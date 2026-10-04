using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    private const double MinimumAdaptiveFrameRate = 10;
    private const double BackoffMultiplier = 1.25;
    private const double RecoveryThresholdMultiplier = 0.75;

    private readonly IRenderer _renderer;
    private readonly TimerUpdateTrigger _updateTrigger;
    private readonly Stopwatch _frameStopwatch = new();
    private readonly List<ArtemisDevice> _devices = [];
    private readonly SKTextureBrush _textureBrush = new(null) {CalculationMode = RenderMode.Absolute};

    private ListLedGroup? _surfaceLedGroup;
    private SKTexture? _texture;
    private double _effectiveFrameRate;

    public SurfaceManager(IRenderer renderer, IManagedGraphicsContext? graphicsContext, int targetFrameRate, float renderScale)
    {
        _renderer = renderer;
        _updateTrigger = new TimerUpdateTrigger(false) {UpdateFrequency = 1.0 / targetFrameRate};

        GraphicsContext = graphicsContext;
        TargetFrameRate = targetFrameRate;
        _effectiveFrameRate = targetFrameRate;
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
        List<IRGBDevice> newDevices = [];
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
        List<IRGBDevice> removedDevices = [];
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
        SetEffectiveFrameRate(TargetFrameRate);
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
        _frameStopwatch.Restart();
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
            _frameStopwatch.Stop();
            UpdateAdaptiveFrameRate(_frameStopwatch.Elapsed);
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

    private void UpdateAdaptiveFrameRate(TimeSpan frameTime)
    {
        if (TargetFrameRate <= MinimumAdaptiveFrameRate)
            return;

        double targetFrameTimeMs = 1000.0 / TargetFrameRate;
        double frameTimeMs = frameTime.TotalMilliseconds;

        if (frameTimeMs > targetFrameTimeMs)
        {
            // A synchronous GPU readback that misses its budget should yield the next
            // compositor tick instead of immediately competing with the foreground app.
            double sustainableFrameRate = 1000.0 / (frameTimeMs * BackoffMultiplier);
            SetEffectiveFrameRate(Math.Max(MinimumAdaptiveFrameRate, sustainableFrameRate));
            return;
        }

        if (_effectiveFrameRate < TargetFrameRate && frameTimeMs <= targetFrameTimeMs * RecoveryThresholdMultiplier)
            SetEffectiveFrameRate(Math.Min(TargetFrameRate, _effectiveFrameRate * 1.15));
    }

    private void SetEffectiveFrameRate(double frameRate)
    {
        frameRate = Math.Clamp(frameRate, Math.Min(MinimumAdaptiveFrameRate, TargetFrameRate), TargetFrameRate);
        if (Math.Abs(_effectiveFrameRate - frameRate) < 0.1)
            return;

        _effectiveFrameRate = frameRate;
        _updateTrigger.UpdateFrequency = 1.0 / _effectiveFrameRate;
    }
}
