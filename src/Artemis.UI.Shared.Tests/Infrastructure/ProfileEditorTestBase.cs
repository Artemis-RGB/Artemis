using System.Text.Json;
using Artemis.Core.LayerBrushes;
using Artemis.Core.LayerEffects;
using Artemis.Core.Tests.Infrastructure.Rendering;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Tests.Infrastructure.Services;
using Serilog.Core;

namespace Artemis.UI.Shared.Tests.Infrastructure;

/// <summary>
///     Base class for profile editor tests. Builds on <see cref="RenderTestBase" />, so <see cref="RenderTestBase.Harness" />
///     and a single ANSI keyboard are available, and adds a profile, a <see cref="ProfileEditorService" /> backed by test
///     services and helpers for comparing profiles before and after a command.
/// </summary>
public abstract class ProfileEditorTestBase : RenderTestBase
{
    private readonly Profile _scratchProfile;

    protected ProfileEditorTestBase()
    {
        Profile = Harness.CreateProfile();
        _scratchProfile = Harness.CreateProfile("Scratch profile");
        DeviceService.EnabledDeviceList.Add(Keyboard);

        Service = new ProfileEditorService(Logger.None, DeviceService, ProfileService, ModuleService, LayerBrushService, MainWindowService, WindowService);
    }

    /// <summary>
    ///     Gets the profile the test works on, created with <see cref="RenderHarness.CreateProfile" />
    /// </summary>
    protected Profile Profile { get; }

    /// <summary>
    ///     Gets the root folder of <see cref="Profile" />
    /// </summary>
    protected Folder Root => Profile.GetRootFolder();

    /// <summary>
    ///     Gets the profile configuration of <see cref="Profile" />
    /// </summary>
    protected ProfileConfiguration Configuration => Profile.Configuration;

    protected IProfileEditorService Service { get; }
    protected TestProfileService ProfileService { get; } = new();
    protected TestModuleService ModuleService { get; } = new();
    protected TestDeviceService DeviceService { get; } = new();
    protected TestLayerBrushService LayerBrushService { get; } = new();
    protected TestMainWindowService MainWindowService { get; } = new();
    protected TestWindowService WindowService { get; } = new();

    /// <summary>
    ///     Saves <see cref="Profile" /> the way Artemis stores it and returns the stored JSON. Comparing the snapshot taken
    ///     before a command with the one taken after undoing it shows whether undo restored everything that gets saved.
    /// </summary>
    protected string Snapshot()
    {
        return JsonSerializer.Serialize(Harness.Save(Profile));
    }

    /// <summary>
    ///     Adds a layer with a <see cref="Core.Tests.Infrastructure.Plugins.LayerBrushes.SolidColorTestBrush" /> covering the
    ///     given keyboard LEDs to the provided folder, or the root folder.
    /// </summary>
    protected Layer AddLayer(string name, Folder? parent = null, params RGB.NET.Core.LedId[] leds)
    {
        return Harness.AddLayer<Core.Tests.Infrastructure.Plugins.LayerBrushes.SolidColorTestBrush>(parent ?? Root, Leds(leds), name).Layer;
    }

    /// <summary>
    ///     Gets the descriptor of a test brush, used by commands that create brushes
    /// </summary>
    protected LayerBrushDescriptor BrushDescriptor<TBrush>() where TBrush : BaseLayerBrush
    {
        Layer scratchLayer = new(_scratchProfile.GetRootFolder(), "Scratch layer");
        _scratchProfile.GetRootFolder().AddChild(scratchLayer);
        return Harness.ChangeBrush<TBrush>(scratchLayer).Descriptor;
    }

    /// <summary>
    ///     Creates a new test effect for the provided element without adding it, the way the editor creates effects before
    ///     handing them to <see cref="Services.ProfileEditor.Commands.AddLayerEffect" />
    /// </summary>
    protected TEffect NewEffect<TEffect>(RenderProfileElement element) where TEffect : BaseLayerEffect
    {
        Layer scratchLayer = new(_scratchProfile.GetRootFolder(), "Scratch layer");
        _scratchProfile.GetRootFolder().AddChild(scratchLayer);
        LayerEffectDescriptor descriptor = Harness.AddEffect<TEffect>(scratchLayer).Descriptor!;
        return (TEffect) descriptor.CreateInstance(element, null);
    }

    /// <summary>
    ///     Gets whether the properties of a brush or effect were disposed, which happens when the brush or effect is disposed
    /// </summary>
    protected static bool IsDisposed(LayerPropertyGroup properties)
    {
        try
        {
            properties.GetAllLayerProperties();
            return false;
        }
        catch (ObjectDisposedException)
        {
            return true;
        }
    }

    /// <summary>
    ///     Makes <see cref="Profile" /> the profile being edited, like opening it in the profile editor
    /// </summary>
    protected async Task StartEditing()
    {
        await Service.ChangeCurrentProfileConfiguration(Configuration);
    }

    /// <summary>
    ///     Gets the value an observable of the service currently holds
    /// </summary>
    protected static T Current<T>(IObservable<T> observable)
    {
        T value = default!;
        bool received = false;
        using (observable.Subscribe(v =>
               {
                   if (received)
                       return;
                   value = v;
                   received = true;
               }))
        {
        }

        if (!received)
            throw new InvalidOperationException("The observable has no current value");
        return value;
    }

    /// <summary>
    ///     Gets the history of the profile configuration currently being edited
    /// </summary>
    protected ProfileEditorHistory CurrentHistory => Current(Service.History) ?? throw new InvalidOperationException("No profile is being edited");

    /// <summary>
    ///     Undoes the last command of the profile being edited
    /// </summary>
    protected IProfileEditorCommand? Undo()
    {
        IProfileEditorCommand? result = null;
        CurrentHistory.Undo.Execute().Subscribe(c => result = c);
        return result;
    }

    /// <summary>
    ///     Redoes the last undone command of the profile being edited
    /// </summary>
    protected IProfileEditorCommand? Redo()
    {
        IProfileEditorCommand? result = null;
        CurrentHistory.Redo.Execute().Subscribe(c => result = c);
        return result;
    }

    /// <summary>
    ///     Waits until the condition is met, for things the service does on another thread
    /// </summary>
    protected static async Task WaitUntil(Func<bool> condition, int timeoutMilliseconds = 5000)
    {
        DateTime end = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
        while (!condition())
        {
            if (DateTime.UtcNow > end)
                throw new TimeoutException("The condition was not met in time");
            await Task.Delay(20, TestContext.Current.CancellationToken);
        }
    }
}
