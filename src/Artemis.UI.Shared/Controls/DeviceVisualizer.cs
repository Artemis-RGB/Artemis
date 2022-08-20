using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Shared.Events;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Threading;
using Avalonia.Visuals.Media.Imaging;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Visualizes an <see cref="ArtemisDevice" /> with optional per-LED colors
    /// </summary>
    public class DeviceVisualizer : Control
    {
        private const double UpdateFrameRate = 25.0;
        private readonly List<DeviceVisualizerLed> _deviceVisualizerLeds;
        private readonly DispatcherTimer _timer;

        private Rect _deviceBounds;
        private RenderTargetBitmap? _deviceImage;
        private List<DeviceVisualizerLed>? _dimmedLeds;
        private List<DeviceVisualizerLed>? _highlightedLeds;
        private ArtemisDevice? _oldDevice;

        /// <inheritdoc />
        public DeviceVisualizer()
        {
            _timer = new DispatcherTimer(DispatcherPriority.Render) {Interval = TimeSpan.FromMilliseconds(1000.0 / UpdateFrameRate)};
            _deviceVisualizerLeds = new List<DeviceVisualizerLed>();

            PointerReleased += OnPointerReleased;
        }

        /// <inheritdoc />
        public override void Render(DrawingContext drawingContext)
        {
            if (Device == null)
                return;

            // Determine the scale required to fit the desired size of the control
            double scale = Math.Min(Bounds.Width / _deviceBounds.Width, Bounds.Height / _deviceBounds.Height);

            DrawingContext.PushedState? boundsPush = null;
            try
            {
                // Scale the visualization in the desired bounding box
                if (Bounds.Width > 0 && Bounds.Height > 0)
                    boundsPush = drawingContext.PushPreTransform(Matrix.CreateScale(scale, scale));

                // Apply device rotation
                using DrawingContext.PushedState translationPush = drawingContext.PushPreTransform(Matrix.CreateTranslation(0 - _deviceBounds.Left, 0 - _deviceBounds.Top));
                using DrawingContext.PushedState rotationPush = drawingContext.PushPreTransform(Matrix.CreateRotation(Matrix.ToRadians(Device.Rotation)));

                // Apply device scale
                using DrawingContext.PushedState scalePush = drawingContext.PushPreTransform(Matrix.CreateScale(Device.Scale, Device.Scale));

                // Render device and LED images 
                if (_deviceImage != null)
                {
                    drawingContext.DrawImage(
                        _deviceImage,
                        new Rect(_deviceImage.Size),
                        new Rect(0, 0, Device.RgbDevice.ActualSize.Width, Device.RgbDevice.ActualSize.Height),
                        RenderOptions.GetBitmapInterpolationMode(this)
                    );
                }

                if (!ShowColors)
                    return;

                lock (_deviceVisualizerLeds)
                {
                    foreach (DeviceVisualizerLed deviceVisualizerLed in _deviceVisualizerLeds)
                        deviceVisualizerLed.RenderGeometry(drawingContext, false);
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
        /// Occurs when the device was clicked but not on a LED.
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

        private void Update()
        {
            InvalidateVisual();
        }

        private Rect MeasureDevice()
        {
            if (Device == null)
                return Rect.Empty;

            Rect deviceRect = new(0, 0, Device.RgbDevice.ActualSize.Width, Device.RgbDevice.ActualSize.Height);
            Geometry geometry = new RectangleGeometry(deviceRect);
            geometry.Transform = new RotateTransform(Device.Rotation);

            return geometry.Bounds;
        }

        private void TimerOnTick(object? sender, EventArgs e)
        {
            if (ShowColors && IsVisible && Opacity > 0)
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
            AvaloniaProperty.Register<DeviceVisualizer, ArtemisDevice?>(nameof(Device), notifying: DeviceUpdated);

        private static void DeviceUpdated(IAvaloniaObject sender, bool before)
        {
            if (!before)
                ((DeviceVisualizer) sender).SetupForDevice();
        }

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

        /// <summary>
        ///     Gets or sets a list of LEDs to highlight
        /// </summary>
        public static readonly StyledProperty<ObservableCollection<ArtemisLed>?> HighlightedLedsProperty =
            AvaloniaProperty.Register<DeviceVisualizer, ObservableCollection<ArtemisLed>?>(nameof(HighlightedLeds));

        /// <summary>
        ///     Gets or sets a list of LEDs to highlight
        /// </summary>
        public ObservableCollection<ArtemisLed>? HighlightedLeds
        {
            get => GetValue(HighlightedLedsProperty);
            set => SetValue(HighlightedLedsProperty, value);
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
            _highlightedLeds = new List<DeviceVisualizerLed>();
            _dimmedLeds = new List<DeviceVisualizerLed>();

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
            Task.Run(() =>
            {
                if (device.Layout?.Image == null || !File.Exists(device.Layout.Image.LocalPath))
                {
                    _deviceImage?.Dispose();
                    _deviceImage = null;
                    return;
                }

                try
                {
                    // Create a bitmap that'll be used to render the device and LED images just once
                    RenderTargetBitmap renderTargetBitmap = new(new PixelSize((int) device.RgbDevice.Size.Width * 4, (int) device.RgbDevice.Size.Height * 4));

                    using IDrawingContextImpl context = renderTargetBitmap.CreateDrawingContext(new ImmediateRenderer(this));
                    using Bitmap bitmap = new(device.Layout.Image.LocalPath);
                    context.DrawBitmap(bitmap.PlatformImpl, 1, new Rect(bitmap.Size), new Rect(renderTargetBitmap.Size), BitmapInterpolationMode.HighQuality);
                    lock (_deviceVisualizerLeds)
                    {
                        foreach (DeviceVisualizerLed deviceVisualizerLed in _deviceVisualizerLeds)
                            deviceVisualizerLed.DrawBitmap(context);
                    }

                    _deviceImage?.Dispose();
                    _deviceImage = renderTargetBitmap;

                    Dispatcher.UIThread.Post(InvalidateMeasure);
                }
                catch
                {
                    // ignored
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
}