using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.SkiaSharp;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.Services.Core;

/// <summary>
/// An engine drivers an update loop for a set of devices using a graphics context
/// </summary>
internal sealed class SurfaceManager : IDisposable
{
    private readonly IRenderer _renderer;
    private readonly TimerUpdateTrigger _updateTrigger;
    private readonly List<ArtemisDevice> _devices = new();
    private readonly SKTextureBrush _textureBrush = new(null) {CalculationMode = RenderMode.Absolute};

    private ListLedGroup? _surfaceLedGroup;
    private SKTexture? _texture;

    public SurfaceManager(IRenderer renderer, IManagedGraphicsContext? graphicsContext, int targetFrameRate, float renderScale)
    {
        _renderer = renderer;
        _updateTrigger = new TimerUpdateTrigger(false) {UpdateFrequency = 1.0 / targetFrameRate};

        GraphicsContext = graphicsContext;
        TargetFrameRate = targetFrameRate;
        RenderScale = renderScale;
        Surface = new RGBSurface();
        Surface.Updating += SurfaceOnUpdating;
        Surface.RegisterUpdateTrigger(_updateTrigger);

        SetPaused(true);
    }

    public IManagedGraphicsContext? GraphicsContext { get; private set; }
    public int TargetFrameRate { get; private set; }
    public float RenderScale { get; private set; }
    public RGBSurface Surface { get; }

    public bool IsPaused { get; private set; }

    public void AddDevices(IEnumerable<ArtemisDevice> devices)
    {
        List<IRGBDevice> newDevices = new();
        lock (_devices)
        {
            foreach (ArtemisDevice artemisDevice in devices)
            {
                if (_devices.Contains(artemisDevice))
                    continue;
                _devices.Add(artemisDevice);
                newDevices.Add(artemisDevice.RgbDevice);
                artemisDevice.DeviceUpdated += ArtemisDeviceOnDeviceUpdated;
            }
        }

        if (!newDevices.Any())
            return;
        
        Surface.Attach(newDevices);
        _texture?.Invalidate();
    }

    public void RemoveDevices(IEnumerable<ArtemisDevice> devices)
    {
        List<IRGBDevice> removedDevices = new();
        lock (_devices)
        {
            foreach (ArtemisDevice artemisDevice in devices)
            {
                if (!_devices.Remove(artemisDevice))
                    continue;
                artemisDevice.DeviceUpdated -= ArtemisDeviceOnDeviceUpdated;
                removedDevices.Add(artemisDevice.RgbDevice);
                _devices.Remove(artemisDevice);
            }
        }

        if (!removedDevices.Any())
            return;
        
        Surface.Detach(removedDevices);
        _texture?.Invalidate();
    }

    public bool SetPaused(bool paused)
    {
        if (IsPaused == paused)
            return false;

        if (paused)
            _updateTrigger.Stop();
        else
            _updateTrigger.Start();

        IsPaused = paused;
        return true;
    }

    public void UpdateTargetFrameRate(int targetFrameRate)
    {
        TargetFrameRate = targetFrameRate;
        _updateTrigger.UpdateFrequency = 1.0 / TargetFrameRate;
    }

    public void UpdateRenderScale(float renderScale)
    {
        RenderScale = renderScale;
        _texture?.Invalidate();
    }

    public void UpdateGraphicsContext(IManagedGraphicsContext? graphicsContext)
    {
        GraphicsContext = graphicsContext;
        _texture?.Invalidate();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        SetPaused(true);
        Surface.UnregisterUpdateTrigger(_updateTrigger);

        _updateTrigger.Dispose();
        _texture?.Dispose();
        Surface.Dispose();
    }

    private SKTexture CreateTexture()
    {
        float evenWidth = Surface.Boundary.Size.Width;
        if (evenWidth % 2 != 0)
            evenWidth++;
        float evenHeight = Surface.Boundary.Size.Height;
        if (evenHeight % 2 != 0)
            evenHeight++;

        int width = Math.Max(1, MathF.Min(evenWidth * RenderScale, 4096).RoundToInt());
        int height = Math.Max(1, MathF.Min(evenHeight * RenderScale, 4096).RoundToInt());

        lock (_devices)
        {
            _texture?.Dispose();
            _texture = new SKTexture(GraphicsContext, width, height, RenderScale, _devices);
            _textureBrush.Texture = _texture;

            _surfaceLedGroup?.Detach();
            _surfaceLedGroup = new ListLedGroup(Surface, _devices.SelectMany(d => d.Leds).Select(l => l.RgbLed)) {Brush = _textureBrush};
        }

        return _texture;
    }

    private void SurfaceOnUpdating(UpdatingEventArgs args)
    {
        SKTexture? texture = _texture;
        if (texture == null || texture.IsInvalid)
            texture = CreateTexture();

        // Prepare a canvas
        SKCanvas canvas = texture.Surface.Canvas;
        canvas.Save();

        // Apply scaling if necessary
        if (Math.Abs(texture.RenderScale - 1) > 0.001)
            canvas.Scale(texture.RenderScale);

        // Fresh start!
        canvas.Clear(new SKColor(0, 0, 0));

        try
        {
            _renderer.Render(canvas, args.DeltaTime);
        }
        finally
        {
            canvas.RestoreToCount(-1);
            canvas.Flush();
            texture.CopyPixelData();
        }

        try
        {
            _renderer.PostRender(texture);
        }
        catch
        {
            // ignored
        }
    }

    private void ArtemisDeviceOnDeviceUpdated(object? sender, EventArgs e)
    {
        _texture?.Invalidate();
    }
}