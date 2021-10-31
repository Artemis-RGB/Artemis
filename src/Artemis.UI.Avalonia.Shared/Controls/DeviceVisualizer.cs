using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Avalonia.Shared.Events;
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

namespace Artemis.UI.Avalonia.Shared.Controls
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
                    boundsPush = drawingContext.PushPostTransform(Matrix.CreateScale(scale, scale));

                // Apply device rotation
                using DrawingContext.PushedState translationPush = drawingContext.PushPostTransform(Matrix.CreateTranslation(0 - _deviceBounds.Left, 0 - _deviceBounds.Top));
                using DrawingContext.PushedState rotationPush = drawingContext.PushPostTransform(Matrix.CreateRotation(Device.Rotation));

                // Apply device scale
                using DrawingContext.PushedState scalePush = drawingContext.PushPostTransform(Matrix.CreateScale(Device.Scale, Device.Scale));

                // Render device and LED images 
                if (_deviceImage != null)
                    drawingContext.DrawImage(_deviceImage, new Rect(0, 0, Device.RgbDevice.ActualSize.Width, Device.RgbDevice.ActualSize.Height));

                foreach (DeviceVisualizerLed deviceVisualizerLed in _deviceVisualizerLeds)
                    deviceVisualizerLed.RenderGeometry(drawingContext, false);
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

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            _deviceImage?.Dispose();
            _deviceImage = null;
            base.OnDetachedFromVisualTree(e);
        }

        /// <summary>
        ///     Invokes the <see cref="LedClicked" /> event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnLedClicked(LedClickedEventArgs e)
        {
            LedClicked?.Invoke(this, e);
        }

        private void Update()
        {
            InvalidateVisual();
        }

        private void UpdateTransform()
        {
            InvalidateVisual();
            InvalidateMeasure();
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
                OnLedClicked(new LedClickedEventArgs(deviceVisualizerLed.Led.Device, deviceVisualizerLed.Led));
        }

        private void DevicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Dispatcher.UIThread.Post(SetupForDevice);
        }

        private void DeviceUpdated(object? sender, EventArgs e)
        {
            Dispatcher.UIThread.Post(SetupForDevice);
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
            _deviceImage = null;
            _deviceVisualizerLeds.Clear();
            _highlightedLeds = new List<DeviceVisualizerLed>();
            _dimmedLeds = new List<DeviceVisualizerLed>();

            if (Device == null)
                return;

            if (_oldDevice != null)
            {
                Device.RgbDevice.PropertyChanged -= DevicePropertyChanged;
                Device.DeviceUpdated -= DeviceUpdated;
            }

            _oldDevice = Device;
            _deviceBounds = MeasureDevice();

            Device.RgbDevice.PropertyChanged += DevicePropertyChanged;
            Device.DeviceUpdated += DeviceUpdated;
            UpdateTransform();

            // Create all the LEDs
            foreach (ArtemisLed artemisLed in Device.Leds)
                _deviceVisualizerLeds.Add(new DeviceVisualizerLed(artemisLed));

            // Load the device main image
            if (Device.Layout?.Image != null && File.Exists(Device.Layout.Image.LocalPath))
            {
                try
                {
                    // Create a bitmap that'll be used to render the device and LED images just once
                    RenderTargetBitmap renderTargetBitmap = new(new PixelSize((int) Device.RgbDevice.Size.Width * 4, (int) Device.RgbDevice.Size.Height * 4));

                    using IDrawingContextImpl context = renderTargetBitmap.CreateDrawingContext(new ImmediateRenderer(this));
                    using Bitmap bitmap = new(Device.Layout.Image.LocalPath);
                    context.DrawBitmap(bitmap.PlatformImpl, 1, new Rect(bitmap.Size), new Rect(renderTargetBitmap.Size), BitmapInterpolationMode.HighQuality);
                    foreach (DeviceVisualizerLed deviceVisualizerLed in _deviceVisualizerLeds)
                        deviceVisualizerLed.DrawBitmap(context);

                    _deviceImage = renderTargetBitmap;
                }
                catch
                {
                    // ignored
                }
            }

            InvalidateMeasure();
        }

        #region Overrides of Layoutable

        /// <inheritdoc />
        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(Math.Min(availableSize.Width, _deviceBounds.Width), Math.Min(availableSize.Height, _deviceBounds.Height));
        }

        #endregion

        #endregion
    }
}