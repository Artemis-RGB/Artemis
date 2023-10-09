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
    private readonly object _renderLock = new();

    private ListLedGroup? _surfaceLedGroup;
    private SKTexture? _texture;

    public SurfaceManager(IRenderer renderer, IManagedGraphicsContext? graphicsContext, int targetFrameRate, float renderScale)
    {
        _renderer = renderer;
        _updateTrigger = new TimerUpdateTrigger {UpdateFrequency = 1.0 / targetFrameRate};

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
        lock (_renderLock)
        {
            foreach (ArtemisDevice artemisDevice in devices)
            {
                if (_devices.Contains(artemisDevice))
                    continue;
                _devices.Add(artemisDevice);
                Surface.Attach(artemisDevice.RgbDevice);
                artemisDevice.DeviceUpdated += ArtemisDeviceOnDeviceUpdated;
            }

            _texture?.Invalidate();
        }
    }

    public void RemoveDevices(IEnumerable<ArtemisDevice> devices)
    {
        lock (_renderLock)
        {
            foreach (ArtemisDevice artemisDevice in devices)
            {
                artemisDevice.DeviceUpdated -= ArtemisDeviceOnDeviceUpdated;
                Surface.Detach(artemisDevice.RgbDevice);
                _devices.Remove(artemisDevice);
            }

            _texture?.Invalidate();
        }
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
        lock (_renderLock)
        {
            RenderScale = renderScale;
            _texture?.Invalidate();
        }
    }

    public void UpdateGraphicsContext(IManagedGraphicsContext? graphicsContext)
    {
        lock (_renderLock)
        {
            GraphicsContext = graphicsContext;
            _texture?.Invalidate();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        SetPaused(true);
        Surface.UnregisterUpdateTrigger(_updateTrigger);
        lock (_renderLock)
        {
            _updateTrigger.Dispose();
            _texture?.Dispose();
            Surface.Dispose();
        }
    }

    private void UpdateLedGroup()
    {
        List<Led> leds = _devices.SelectMany(d => d.Leds).Select(l => l.RgbLed).ToList();

        if (_surfaceLedGroup == null)
        {
            _surfaceLedGroup = new ListLedGroup(Surface, leds) {Brush = _textureBrush};
            return;
        }

        // Clean up the old background
        _surfaceLedGroup.Detach();

        // Apply the application wide brush and decorator
        _surfaceLedGroup = new ListLedGroup(Surface, leds) {Brush = _textureBrush};
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

        _texture?.Dispose();
        _texture = new SKTexture(GraphicsContext, width, height, RenderScale, _devices);
        _textureBrush.Texture = _texture;

        return _texture;
    }

    private void SurfaceOnUpdating(UpdatingEventArgs args)
    {
        lock (_renderLock)
        {
            SKTexture? texture = _texture;
            if (texture == null || texture.IsInvalid)
            {
                texture = CreateTexture();
                UpdateLedGroup();
            }

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
    }

    private void ArtemisDeviceOnDeviceUpdated(object? sender, EventArgs e)
    {
        _texture?.Invalidate();
    }
}