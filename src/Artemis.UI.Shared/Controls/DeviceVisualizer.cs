using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Shared.Events;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using RGB.NET.Core;
using Color = RGB.NET.Core.Color;
using Point = Avalonia.Point;
using Size = Avalonia.Size;

namespace Artemis.UI.Shared;

/// <summary>
///     Visualizes an <see cref="ArtemisDevice" /> with optional per-LED colors
/// </summary>
public class DeviceVisualizer : Control
{
    private const double UPDATE_FRAME_RATE = 25.0;
    private readonly List<DeviceVisualizerLed> _deviceVisualizerLeds;
    private readonly DispatcherTimer _timer;

    private Rect _deviceBounds;
    private RenderTargetBitmap? _deviceImage;
    private ArtemisDevice? _oldDevice;
    private bool _loading;
    private Color[] _previousState = Array.Empty<Color>();
    
    /// <inheritdoc />
    public DeviceVisualizer()
    {
        _timer = new DispatcherTimer(DispatcherPriority.Background) {Interval = TimeSpan.FromMilliseconds(1000.0 / UPDATE_FRAME_RATE)};
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

        Color[] state = new Color[Device.RgbDevice.Count()];
        bool difference = _previousState.Length != state.Length;

        // Check all LEDs for differences and copy the colors to a new state
        int index = 0;
        foreach (Led led in Device.RgbDevice)
        {
            if (!difference && !led.Color.Equals(_previousState[index]))
                difference = true;

            state[index] = led.Color;
            index++;
        }

        // Store the new state for next time
        _previousState = state;

        return difference;
    }

    private void Update()
    {
        InvalidateVisual();
    }

    private Rect MeasureDevice()
    {
        if (Device == null)
            return new Rect();

        Rect deviceRect = new(0, 0, Device.RgbDevice.ActualSize.Width, Device.RgbDevice.ActualSize.Height);
        Geometry geometry = new RectangleGeometry(deviceRect);
        geometry.Transform = new RotateTransform(Device.Rotation);

        return geometry.Bounds;
    }

    private void TimerOnTick(object? sender, EventArgs e)
    {
        if (IsDirty() && ShowColors && IsVisible && Opacity > 0)
            Update();
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

    private void DevicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(SetupForDevice, DispatcherPriority.Background);
    }

    private void DeviceUpdated(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(SetupForDevice, DispatcherPriority.Background);
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
        _deviceImage?.Dispose();
        _deviceImage = null;

        if (Device != null)
        {
            Device.RgbDevice.PropertyChanged -= DevicePropertyChanged;
            Device.DeviceUpdated -= DeviceUpdated;
        }

        base.OnDetachedFromVisualTree(e);
    }

    /// <inheritdoc />
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _timer.Start();
        _timer.Tick += TimerOnTick;
        base.OnAttachedToLogicalTree(e);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _timer.Stop();
        _timer.Tick -= TimerOnTick;
        base.OnDetachedFromLogicalTree(e);
    }

    private void SetupForDevice()
    {
        lock (_deviceVisualizerLeds)
        {
            _deviceVisualizerLeds.Clear();
        }

        if (_oldDevice != null)
        {
            _oldDevice.RgbDevice.PropertyChanged -= DevicePropertyChanged;
            _oldDevice.DeviceUpdated -= DeviceUpdated;
        }

        _oldDevice = Device;
        if (Device == null)
            return;

        _deviceBounds = MeasureDevice();
        _loading = true;

        Device.RgbDevice.PropertyChanged += DevicePropertyChanged;
        Device.DeviceUpdated += DeviceUpdated;

        // Create all the LEDs
        lock (_deviceVisualizerLeds)
        {
            foreach (ArtemisLed artemisLed in Device.Leds)
                _deviceVisualizerLeds.Add(new DeviceVisualizerLed(artemisLed));
        }

        // Load the device main image on a background thread
        ArtemisDevice? device = Device;
        Dispatcher.UIThread.Post(() =>
        {
            try
            {
                if (device.Layout?.Image == null || !File.Exists(device.Layout.Image.LocalPath))
                {
                    _deviceImage?.Dispose();
                    _deviceImage = null;
                    return;
                }

                // Create a bitmap that'll be used to render the device and LED images just once
                // Render 4 times the actual size of the device to make sure things look sharp when zoomed in
                RenderTargetBitmap renderTargetBitmap = new(new PixelSize((int) device.RgbDevice.ActualSize.Width * 2, (int) device.RgbDevice.ActualSize.Height * 2));

                using DrawingContext context = renderTargetBitmap.CreateDrawingContext();
                using Bitmap bitmap = new(device.Layout.Image.LocalPath);
                using Bitmap scaledBitmap = bitmap.CreateScaledBitmap(renderTargetBitmap.PixelSize);

                context.DrawImage(scaledBitmap, new Rect(scaledBitmap.Size));
                lock (_deviceVisualizerLeds)
                {
                    foreach (DeviceVisualizerLed deviceVisualizerLed in _deviceVisualizerLeds)
                        deviceVisualizerLed.DrawBitmap(context, 2 * Device.Scale);
                }

                _deviceImage?.Dispose();
                _deviceImage = renderTargetBitmap;

                InvalidateMeasure();
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                _loading = false;
            }
        });
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