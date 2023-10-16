using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Events;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using RGB.NET.Core;
using DryIoc;
using Color = RGB.NET.Core.Color;
using Point = Avalonia.Point;
using Size = Avalonia.Size;

namespace Artemis.UI.Shared;

/// <summary>
///     Visualizes an <see cref="ArtemisDevice" /> with optional per-LED colors
/// </summary>
public class DeviceVisualizer : Control
{
    internal static readonly Dictionary<string, RenderTargetBitmap?> BitmapCache = new();
    private readonly IRenderService _renderService;
    private readonly List<DeviceVisualizerLed> _deviceVisualizerLeds;

    private Rect _deviceBounds;
    private RenderTargetBitmap? _deviceImage;
    private ArtemisDevice? _oldDevice;
    private bool _loading;
    private Color[] _previousState = Array.Empty<Color>();

    /// <inheritdoc />
    public DeviceVisualizer()
    {
        _renderService = UI.Locator.Resolve<IRenderService>();
        _deviceVisualizerLeds = new List<DeviceVisualizerLed>();

        PointerReleased += OnPointerReleased;
        PropertyChanged += OnPropertyChanged;
    }

    /// <inheritdoc />
    public override void Render(DrawingContext drawingContext)
    {
        if (Device == null || _deviceBounds.Width == 0 || _deviceBounds.Height == 0 || _loading)
            return;

        // Determine the scale required to fit the desired size of the control
        double scale = Math.Min(Bounds.Width / _deviceBounds.Width, Bounds.Height / _deviceBounds.Height);

        DrawingContext.PushedState? boundsPush = null;
        try
        {
            // Scale the visualization in the desired bounding box
            if (Bounds.Width > 0 && Bounds.Height > 0)
                boundsPush = drawingContext.PushTransform(Matrix.CreateScale(scale, scale));

            // Apply device rotation
            using DrawingContext.PushedState translationPush = drawingContext.PushTransform(Matrix.CreateTranslation(0 - _deviceBounds.Left, 0 - _deviceBounds.Top));
            using DrawingContext.PushedState rotationPush = drawingContext.PushTransform(Matrix.CreateRotation(Matrix.ToRadians(Device.Rotation)));

            // Render device and LED images 
            if (_deviceImage != null)
                drawingContext.DrawImage(
                    _deviceImage,
                    new Rect(_deviceImage.Size),
                    new Rect(0, 0, Device.RgbDevice.ActualSize.Width, Device.RgbDevice.ActualSize.Height)
                );

            if (!ShowColors)
                return;

            lock (_deviceVisualizerLeds)
            {
                // Apply device scale
                using DrawingContext.PushedState scalePush = drawingContext.PushTransform(Matrix.CreateScale(Device.Scale, Device.Scale));
                foreach (DeviceVisualizerLed deviceVisualizerLed in _deviceVisualizerLeds)
                    deviceVisualizerLed.RenderGeometry(drawingContext);
            }
        }
        finally
        {
            boundsPush?.Dispose();
        }
    }

    /// <summary>
    ///     Occurs when a LED of the device has been clicked
    /// </summary>
    public event EventHandler<LedClickedEventArgs>? LedClicked;

    /// <summary>
    ///     Occurs when the device was clicked but not on a LED.
    /// </summary>
    public event EventHandler<PointerReleasedEventArgs>? Clicked;

    /// <summary>
    ///     Invokes the <see cref="LedClicked" /> event
    /// </summary>
    protected virtual void OnLedClicked(LedClickedEventArgs e)
    {
        LedClicked?.Invoke(this, e);
    }

    /// <summary>
    ///     Invokes the <see cref="Clicked" /> event
    /// </summary>
    protected virtual void OnClicked(PointerReleasedEventArgs e)
    {
        Clicked?.Invoke(this, e);
    }

    private bool IsDirty()
    {
        if (Device == null)
            return false;

        // Device might be modified mid-check, in that case just pretend it was not dirty
        try
        {
            bool difference = false;

            int newLedCount = Device.RgbDevice.Count();
            if (_previousState.Length != newLedCount)
            {
                _previousState = new Color[newLedCount];
                difference = true;
            }

            // Check all LEDs for differences and copy the colors to a new state
            int index = 0;
            foreach (Led led in Device.RgbDevice)
            {
                if (_previousState[index] != led.Color)
                    difference = true;

                _previousState[index] = led.Color;
                index++;
            }

            return difference;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private void Update()
    {
        InvalidateVisual();
    }

    private Rect MeasureDevice()
    {
        if (Device == null || float.IsNaN(Device.RgbDevice.ActualSize.Width) || float.IsNaN(Device.RgbDevice.ActualSize.Height))
            return new Rect();
        
        Rect deviceRect = new(0, 0, Device.RgbDevice.ActualSize.Width, Device.RgbDevice.ActualSize.Height);
        Geometry geometry = new RectangleGeometry(deviceRect);
        geometry.Transform = new RotateTransform(Device.Rotation);

        return geometry.Bounds;
    }

    private void OnFrameRendered(object? sender, FrameRenderedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (ShowColors && IsVisible && Opacity > 0 && IsDirty())
                Update();
        }, DispatcherPriority.Background);
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (Device == null)
            return;

        Point position = e.GetPosition(this);
        double x = position.X / Bounds.Width;
        double y = position.Y / Bounds.Height;

        Point scaledPosition = new(x * Device.Rectangle.Width, y * Device.Rectangle.Height);
        DeviceVisualizerLed? deviceVisualizerLed = _deviceVisualizerLeds.FirstOrDefault(l => l.HitTest(scaledPosition));
        if (deviceVisualizerLed != null)
            OnLedClicked(new LedClickedEventArgs(deviceVisualizerLed.Led.Device, deviceVisualizerLed.Led, e));
        else
            OnClicked(e);
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == DeviceProperty)
            SetupForDevice();
    }

    private void DeviceUpdated(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Invoke(SetupForDevice, DispatcherPriority.Background);
    }

    #region Properties

    /// <summary>
    ///     Gets or sets the <see cref="ArtemisDevice" /> to display
    /// </summary>
    public static readonly StyledProperty<ArtemisDevice?> DeviceProperty =
        AvaloniaProperty.Register<DeviceVisualizer, ArtemisDevice?>(nameof(Device));

    /// <summary>
    ///     Gets or sets the <see cref="ArtemisDevice" /> to display
    /// </summary>
    public ArtemisDevice? Device
    {
        get => GetValue(DeviceProperty);
        set => SetValue(DeviceProperty, value);
    }

    /// <summary>
    ///     Gets or sets boolean indicating  whether or not to show per-LED colors
    /// </summary>
    public static readonly StyledProperty<bool> ShowColorsProperty =
        AvaloniaProperty.Register<DeviceVisualizer, bool>(nameof(ShowColors));

    /// <summary>
    ///     Gets or sets a boolean indicating whether or not to show per-LED colors
    /// </summary>
    public bool ShowColors
    {
        get => GetValue(ShowColorsProperty);
        set => SetValue(ShowColorsProperty, value);
    }

    #endregion

    #region Lifetime management

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (Device != null)
        {
            Device.DeviceUpdated -= DeviceUpdated;
        }

        base.OnDetachedFromVisualTree(e);
    }

    /// <inheritdoc />
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _renderService.FrameRendered += OnFrameRendered;

        base.OnAttachedToLogicalTree(e);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _renderService.FrameRendered -= OnFrameRendered;

        base.OnDetachedFromLogicalTree(e);
    }

    private async Task SetupForDevice()
    {
        lock (_deviceVisualizerLeds)
        {
            _deviceVisualizerLeds.Clear();
        }

        if (_oldDevice != null)
        {
            _oldDevice.DeviceUpdated -= DeviceUpdated;
        }

        _oldDevice = Device;
        if (Device == null)
            return;

        _deviceBounds = MeasureDevice();
        _loading = true;

        Device.DeviceUpdated += DeviceUpdated;

        // Create all the LEDs
        lock (_deviceVisualizerLeds)
        {
            foreach (ArtemisLed artemisLed in Device.Leds)
                _deviceVisualizerLeds.Add(new DeviceVisualizerLed(artemisLed));
        }

        // Load the device main image on a background thread
        ArtemisDevice? device = Device;
        try
        {
            _deviceImage = await Task.Run(() => GetDeviceImage(device));
        }
        catch (Exception e)
        {
            // ignored
        }

        InvalidateMeasure();
        _loading = false;
    }

    private RenderTargetBitmap? GetDeviceImage(ArtemisDevice device)
    {
        string? path = device.Layout?.Image?.LocalPath;
        if (path == null)
            return null;
        
        if (BitmapCache.TryGetValue(path, out RenderTargetBitmap? existingBitmap))
            return existingBitmap;
        if (!File.Exists(path))
        {
            BitmapCache[path] = null;
            return null;
        }

        // Create a bitmap that'll be used to render the device and LED images just once
        // Render 4 times the actual size of the device to make sure things look sharp when zoomed in
        RenderTargetBitmap renderTargetBitmap = new(new PixelSize((int) device.RgbDevice.ActualSize.Width * 2, (int) device.RgbDevice.ActualSize.Height * 2));

        using DrawingContext context = renderTargetBitmap.CreateDrawingContext();
        using Bitmap bitmap = new(path);
        using Bitmap scaledBitmap = bitmap.CreateScaledBitmap(renderTargetBitmap.PixelSize);

        context.DrawImage(scaledBitmap, new Rect(scaledBitmap.Size));
        lock (_deviceVisualizerLeds)
        {
            foreach (DeviceVisualizerLed deviceVisualizerLed in _deviceVisualizerLeds)
                deviceVisualizerLed.DrawBitmap(context, 2 * device.Scale);
        }

        // BitmapCache[path] = renderTargetBitmap;
        return renderTargetBitmap;
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        if (_deviceBounds.Width <= 0 || _deviceBounds.Height <= 0)
            return new Size(0, 0);

        double availableWidth = double.IsInfinity(availableSize.Width) ? _deviceBounds.Width : availableSize.Width;
        double availableHeight = double.IsInfinity(availableSize.Height) ? _deviceBounds.Height : availableSize.Height;
        double bestRatio = Math.Min(availableWidth / _deviceBounds.Width, availableHeight / _deviceBounds.Height);

        return new Size(_deviceBounds.Width * bestRatio, _deviceBounds.Height * bestRatio);
    }

    #endregion
}