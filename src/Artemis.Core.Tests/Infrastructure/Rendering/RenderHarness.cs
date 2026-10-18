using System.Text.Json;
using Artemis.Core.LayerBrushes;
using Artemis.Core.LayerEffects;
using Artemis.Core.Services.Core;
using Artemis.Core.Tests.Infrastructure.Plugins;
using Artemis.Core.Tests.Infrastructure.Plugins.DeviceProviders;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerEffects;
using Artemis.Storage.Entities.Profile;
using RGB.NET.Core;
using RGB.NET.Layout;
using SkiaSharp;

namespace Artemis.Core.Tests.Infrastructure.Rendering;

/// <summary>
///     Renders profiles onto fake RGB.NET devices through Artemis' real render pipeline (<see cref="SurfaceManager" />,
///     <see cref="SKTexture" /> sampling and RGB.NET LED updates) without starting any services or loading plugins.
///     <para>Time is fully controlled by the harness, frames are only rendered when requested.</para>
/// </summary>
public sealed class RenderHarness : IRenderer, IDisposable
{
    /// <summary>
    ///     The render scale Artemis uses by default
    /// </summary>
    public const float DefaultRenderScale = 0.5f;

    private readonly TestPlugin _plugin = new();
    private readonly TestLayerBrushProvider _brushProvider;
    private readonly TestLayerEffectProvider _effectProvider;
    private readonly SurfaceManager _surfaceManager;
    private readonly List<ArtemisDevice> _devices = [];
    private readonly List<Exception> _renderExceptions = [];
    private readonly Dictionary<Led, SKColor> _deviceColors = [];
    private readonly Dictionary<Profile, TimeSpan> _elapsed = [];

    private readonly int _previousRenderScaleMultiplier;

    private Action<SKCanvas>? _draw;
    private SKBitmap? _lastCanvas;
    private bool _disposed;

    /// <param name="renderScale">The scale of the canvas relative to the surface, Artemis defaults to 0.5</param>
    public RenderHarness(float renderScale = DefaultRenderScale)
    {
        // RenderService keeps the static render scale multiplier, which snaps LED rectangles to whole canvas pixels, in sync
        // with the render scale setting. Set it before any devices are created and restore it afterwards
        _previousRenderScaleMultiplier = RenderScale.RenderScaleMultiplier;
        RenderScale.SetRenderScaleMultiplier((int) (1 / renderScale));

        _brushProvider = _plugin.AddFeature(new TestLayerBrushProvider());
        _effectProvider = _plugin.AddFeature(new TestLayerEffectProvider());
        _surfaceManager = new SurfaceManager(this, null, 30, renderScale);

        // RGB.NET swallows exceptions thrown during a surface update, collect them so they can fail the test
        _surfaceManager.Surface.Exception += args => _renderExceptions.Add(args.Exception);
    }

    /// <summary>
    ///     Gets the devices added to the harness
    /// </summary>
    public IReadOnlyList<ArtemisDevice> Devices => _devices;

    /// <summary>
    ///     Adds a fake device based on an RGB.NET layout file, see <see cref="TestLayouts" /> for the layouts shipped with Artemis.
    /// </summary>
    /// <param name="layoutPath">The path to the layout XML file</param>
    /// <param name="x">The X position of the device on the surface</param>
    /// <param name="y">The Y position of the device on the surface</param>
    public ArtemisDevice AddDevice(string layoutPath, float x = 0, float y = 0)
    {
        DeviceLayout layout = DeviceLayout.Load(layoutPath) ?? throw new ArgumentException($"Failed to load layout {layoutPath}", nameof(layoutPath));

        // One provider per device keeps devices independent of each other
        TestDeviceProvider deviceProvider = _plugin.AddFeature(new TestDeviceProvider());
        IRGBDevice rgbDevice = deviceProvider.CreateDevice(layout, CaptureLedColor);

        ArtemisDevice device = new(rgbDevice, deviceProvider) {X = x, Y = y};
        _devices.Add(device);
        _surfaceManager.AddDevices([device]);
        return device;
    }

    /// <summary>
    ///     Removes the device from the surface, like disabling a device in Artemis does.
    /// </summary>
    public void RemoveDevice(ArtemisDevice device)
    {
        _surfaceManager.RemoveDevices([device]);
        _devices.Remove(device);
    }

    /// <summary>
    ///     Creates a new, empty profile containing only a root folder.
    /// </summary>
    public Profile CreateProfile(string name = "Test profile")
    {
        ProfileCategory category = new("Tests", 0);
        ProfileConfiguration configuration = new(category, name, "TestTube");
        Profile profile = new(configuration, configuration.Entity.Profile);
        configuration.Profile = profile;
        return profile;
    }

    /// <summary>
    ///     Adds a layer to the provided parent that uses the provided test brush.
    ///     <para>
    ///         Note: like any layer created in code, its timeline starts at the end. Call <see cref="ProfileElement.Reset" />
    ///         on the profile (like the editor does) or use <see cref="SaveAndReload" /> when a test depends on the timeline
    ///         starting at the beginning.
    ///     </para>
    /// </summary>
    /// <param name="parent">The folder (or profile root folder) to add the layer to</param>
    /// <param name="leds">The LEDs the layer should cover</param>
    /// <param name="name">The name of the layer</param>
    /// <param name="order">
    ///     The position among the parent's children (0-based), defaults to the end. The first child renders on top.
    /// </param>
    /// <typeparam name="TBrush">The type of test brush to use, must be registered by <see cref="TestLayerBrushProvider" /></typeparam>
    /// <returns>The layer and its brush, the brush exposes the brush properties</returns>
    public (Layer Layer, TBrush Brush) AddLayer<TBrush>(Folder parent, IEnumerable<ArtemisLed> leds, string name = "Test layer", int? order = null) where TBrush : BaseLayerBrush
    {
        // Note: despite its documentation the Layer constructor does not add the layer to the parent
        Layer layer = new(parent, name);
        parent.AddChild(layer, order);
        layer.AddLeds(leds);
        return (layer, ChangeBrush<TBrush>(layer));
    }

    /// <summary>
    ///     Gives the layer a new instance of the provided test brush.
    /// </summary>
    /// <typeparam name="TBrush">The type of test brush to use, must be registered by <see cref="TestLayerBrushProvider" /></typeparam>
    public TBrush ChangeBrush<TBrush>(Layer layer) where TBrush : BaseLayerBrush
    {
        LayerBrushDescriptor descriptor = _brushProvider.LayerBrushDescriptors.Single(d => d.LayerBrushType == typeof(TBrush));
        layer.ChangeLayerBrush(descriptor.CreateInstance(layer, null));
        return (TBrush) layer.LayerBrush!;
    }

    /// <summary>
    ///     Adds an empty folder to the provided parent.
    /// </summary>
    /// <param name="parent">The folder (or profile root folder) to add the folder to</param>
    /// <param name="name">The name of the folder</param>
    /// <param name="order">
    ///     The position among the parent's children (0-based), defaults to the end. The first child renders on top.
    /// </param>
    public Folder AddFolder(Folder parent, string name = "Test folder", int? order = null)
    {
        // Like the Layer constructor, the Folder constructor does not add the folder to the parent
        Folder folder = new(parent, name);
        parent.AddChild(folder, order);
        return folder;
    }

    /// <summary>
    ///     Adds a test effect to the provided layer or folder.
    /// </summary>
    /// <typeparam name="TEffect">The type of test effect to use, must be registered by <see cref="TestLayerEffectProvider" /></typeparam>
    public TEffect AddEffect<TEffect>(RenderProfileElement element) where TEffect : BaseLayerEffect
    {
        LayerEffectDescriptor descriptor = _effectProvider.LayerEffectDescriptors.Single(d => d.LayerEffectType == typeof(TEffect));
        TEffect effect = (TEffect) descriptor.CreateInstance(element, null);
        element.AddLayerEffect(effect);
        return effect;
    }

    /// <summary>
    ///     Saves the profile and loads it back into a new profile instance, the way Artemis stores and activates profiles.
    /// </summary>
    /// <returns>The reloaded profile, its time starts at zero</returns>
    public Profile SaveAndReload(Profile profile)
    {
        return Load(Save(profile));
    }

    /// <summary>
    ///     Saves the profile the way Artemis stores it.
    /// </summary>
    /// <returns>A copy of the stored entity, detached from the profile</returns>
    public ProfileEntity Save(Profile profile)
    {
        // Mirrors ProfileService.SaveProfile
        profile.Save();
        foreach (RenderProfileElement element in profile.GetAllRenderElements())
            element.Save();

        // The database stores the profile entity as JSON
        return JsonSerializer.Deserialize<ProfileEntity>(JsonSerializer.Serialize(profile.ProfileEntity))!;
    }

    /// <summary>
    ///     Loads a stored profile onto the devices of this harness, the way Artemis activates profiles.
    /// </summary>
    public Profile Load(ProfileEntity entity, string name = "Test profile")
    {
        // Mirrors ProfileService.ActivateProfile
        ProfileCategory category = new("Tests", 0);
        ProfileConfiguration configuration = new(category, name, "TestTube");
        configuration.Entity.Profile = entity;
        Profile profile = new(configuration, entity);
        profile.PopulateLeds(_devices);
        configuration.Profile = profile;
        return profile;
    }

    /// <summary>
    ///     Advances the profile by <paramref name="delta" /> and renders a single frame.
    /// </summary>
    public RenderFrame Render(Profile profile, TimeSpan delta)
    {
        // Layers skip updating their properties when their timeline did not move, Artemis itself never renders a zero delta
        if (delta <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(delta), delta, "Frames must advance time, layers don't update their properties on a zero delta");

        TimeSpan time = GetElapsed(profile) + delta;
        RenderFrame frame = RenderSurface(time, canvas =>
        {
            profile.Update(delta.TotalSeconds);
            profile.Render(canvas, SKPointI.Empty, null);
        });

        if (profile.Exceptions.Count > 0)
            throw new AggregateException("The profile threw exceptions while rendering", profile.Exceptions);

        _elapsed[profile] = time;
        return frame;
    }

    /// <summary>
    ///     Renders a frame by drawing directly onto the canvas instead of rendering a profile, for testing the render pipeline
    ///     itself. Like a profile, the drawing uses surface coordinates, the canvas is already scaled to the render scale.
    /// </summary>
    public RenderFrame Render(Action<SKCanvas> draw)
    {
        return RenderSurface(TimeSpan.Zero, draw);
    }

    private RenderFrame RenderSurface(TimeSpan time, Action<SKCanvas> draw)
    {
        _draw = draw;
        _renderExceptions.Clear();
        try
        {
            // Synchronously runs SurfaceManager's update which calls back into Render and PostRender below
            _surfaceManager.Surface.Update(true);
        }
        finally
        {
            _draw = null;
        }

        if (_renderExceptions.Count > 0)
            throw new AggregateException("The surface threw exceptions while rendering", _renderExceptions);

        // LEDs that were not sent a color this frame keep showing their previous color, like real hardware
        Dictionary<ArtemisLed, SKColor> ledColors = _devices
            .SelectMany(d => d.Leds)
            .ToDictionary(l => l, l => _deviceColors.TryGetValue(l.RgbLed, out SKColor color) ? color : SKColors.Transparent);

        SKBitmap canvas = _lastCanvas ?? throw new InvalidOperationException("The surface did not render a frame");
        _lastCanvas = null;
        return new RenderFrame(time, canvas, ledColors);
    }

    /// <summary>
    ///     Renders the profile at each of the provided timestamps, advancing time between them.
    ///     <para>
    ///         Timestamps are the total time the harness rendered the profile for, each must be later than the time the
    ///         profile is currently at.
    ///     </para>
    /// </summary>
    public RenderTimeline RenderTimeline(Profile profile, params TimeSpan[] timestamps)
    {
        List<RenderFrame> frames = [];
        foreach (TimeSpan timestamp in timestamps.Order())
            frames.Add(Render(profile, timestamp - GetElapsed(profile)));

        return new RenderTimeline(frames);
    }

    /// <summary>
    ///     Gets the total time the harness rendered the profile for
    /// </summary>
    public TimeSpan GetElapsed(Profile profile)
    {
        return _elapsed.GetValueOrDefault(profile);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _surfaceManager.Dispose();
        _plugin.Dispose();
        RenderScale.SetRenderScaleMultiplier(_previousRenderScaleMultiplier);
    }

    void IRenderer.Render(SKCanvas canvas, double delta)
    {
        // Ignore the surface's wall-clock delta, the harness controls time
        _draw?.Invoke(canvas);
    }

    void IRenderer.PostRender(SKTexture texture)
    {
        // Ownership of the bitmap moves to the RenderFrame
        using SKImage snapshot = texture.Surface.Snapshot();
        _lastCanvas = SKBitmap.FromImage(snapshot);
    }

    private void CaptureLedColor(Led led, Color color)
    {
        // Called by the test device when the surface flushes LEDs, these are the colors the hardware would receive
        _deviceColors[led] = new SKColor(color.GetR(), color.GetG(), color.GetB(), color.GetA());
    }
}
