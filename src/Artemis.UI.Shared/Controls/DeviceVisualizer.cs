using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Artemis.Core;
using Stylet;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Visualizes an <see cref="ArtemisDevice" /> with optional per-LED colors
    /// </summary>
    public class DeviceVisualizer : FrameworkElement, IDisposable
    {
        /// <summary>
        ///     The device to visualize
        /// </summary>
        public static readonly DependencyProperty DeviceProperty = DependencyProperty.Register(nameof(Device), typeof(ArtemisDevice), typeof(DeviceVisualizer),
            new FrameworkPropertyMetadata(default(ArtemisDevice), FrameworkPropertyMetadataOptions.AffectsRender, DevicePropertyChangedCallback));

        /// <summary>
        ///     Whether or not to show per-LED colors
        /// </summary>
        public static readonly DependencyProperty ShowColorsProperty = DependencyProperty.Register(nameof(ShowColors), typeof(bool), typeof(DeviceVisualizer),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender, ShowColorsPropertyChangedCallback));

        /// <summary>
        ///     A list of LEDs to highlight
        /// </summary>
        public static readonly DependencyProperty HighlightedLedsProperty = DependencyProperty.Register(nameof(HighlightedLeds), typeof(IEnumerable<ArtemisLed>), typeof(DeviceVisualizer),
            new FrameworkPropertyMetadata(default(IEnumerable<ArtemisLed>)));

        private readonly DrawingGroup _backingStore;
        private readonly List<DeviceVisualizerLed> _deviceVisualizerLeds;
        private readonly Timer _timer;
        private BitmapImage? _deviceImage;
        private ArtemisDevice? _oldDevice;

        /// <summary>
        ///     Creates a new instance of the <see cref="DeviceVisualizer" /> class
        /// </summary>
        public DeviceVisualizer()
        {
            _backingStore = new DrawingGroup();
            _deviceVisualizerLeds = new List<DeviceVisualizerLed>();

            // Run an update timer at 25 fps
            _timer = new Timer(40);
            _timer.Elapsed += TimerOnTick;

            MouseLeftButtonUp += OnMouseLeftButtonUp;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        /// <summary>
        ///     Gets or sets the device to visualize
        /// </summary>
        public ArtemisDevice? Device
        {
            get => (ArtemisDevice) GetValue(DeviceProperty);
            set => SetValue(DeviceProperty, value);
        }

        /// <summary>
        ///     Gets or sets whether or not to show per-LED colors
        /// </summary>
        public bool ShowColors
        {
            get => (bool) GetValue(ShowColorsProperty);
            set => SetValue(ShowColorsProperty, value);
        }

        /// <summary>
        ///     Gets or sets a list of LEDs to highlight
        /// </summary>
        public IEnumerable<ArtemisLed>? HighlightedLeds
        {
            get => (IEnumerable<ArtemisLed>) GetValue(HighlightedLedsProperty);
            set => SetValue(HighlightedLedsProperty, value);
        }

        /// <summary>
        ///     Occurs when a LED of the device has been clicked
        /// </summary>
        public event EventHandler<LedClickedEventArgs>? LedClicked;

        /// <inheritdoc />
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (Device == null)
                return;

            // Determine the scale required to fit the desired size of the control
            Size measureSize = MeasureDevice();
            double scale = Math.Min(DesiredSize.Width / measureSize.Width, DesiredSize.Height / measureSize.Height);
            Rect scaledRect = new(0, 0, measureSize.Width * scale, measureSize.Height * scale);

            // Center and scale the visualization in the desired bounding box
            if (DesiredSize.Width > 0 && DesiredSize.Height > 0)
            {
                drawingContext.PushTransform(new TranslateTransform(DesiredSize.Width / 2 - scaledRect.Width / 2, DesiredSize.Height / 2 - scaledRect.Height / 2));
                drawingContext.PushTransform(new ScaleTransform(scale, scale));
            }

            // Determine the offset required to rotate within bounds
            Rect rotationRect = new(0, 0, Device.RgbDevice.ActualSize.Width, Device.RgbDevice.ActualSize.Height);
            rotationRect.Transform(new RotateTransform(Device.Rotation).Value);

            // Apply device rotation
            drawingContext.PushTransform(new TranslateTransform(0 - rotationRect.Left, 0 - rotationRect.Top));
            drawingContext.PushTransform(new RotateTransform(Device.Rotation));

            // Apply device scale
            drawingContext.PushTransform(new ScaleTransform(Device.Scale, Device.Scale));

            // Render device and LED images 
            if (_deviceImage != null)
                drawingContext.DrawImage(_deviceImage, new Rect(0, 0, Device.RgbDevice.Size.Width, Device.RgbDevice.Size.Height));

            foreach (DeviceVisualizerLed deviceVisualizerLed in _deviceVisualizerLeds)
                deviceVisualizerLed.RenderImage(drawingContext);

            drawingContext.DrawDrawing(_backingStore);
        }

        /// <inheritdoc />
        protected override Size MeasureOverride(Size availableSize)
        {
            if (Device == null)
                return Size.Empty;

            Size deviceSize = MeasureDevice();
            if (deviceSize.Width <= 0 || deviceSize.Height <= 0)
                return Size.Empty;

            return ResizeKeepAspect(deviceSize, availableSize.Width, availableSize.Height);
        }

        /// <summary>
        ///     Invokes the <see cref="LedClicked" /> event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnLedClicked(LedClickedEventArgs e)
        {
            LedClicked?.Invoke(this, e);
        }

        /// <summary>
        ///     Releases the unmanaged resources used by the object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <see langword="true" /> to release both managed and unmanaged resources;
        ///     <see langword="false" /> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing) _timer.Stop();
        }


        private static Size ResizeKeepAspect(Size src, double maxWidth, double maxHeight)
        {
            double scale;
            if (double.IsPositiveInfinity(maxWidth) && !double.IsPositiveInfinity(maxHeight))
                scale = maxHeight / src.Height;
            else if (!double.IsPositiveInfinity(maxWidth) && double.IsPositiveInfinity(maxHeight))
                scale = maxWidth / src.Width;
            else if (double.IsPositiveInfinity(maxWidth) && double.IsPositiveInfinity(maxHeight))
                return src;
            else
                scale = Math.Min(maxWidth / src.Width, maxHeight / src.Height);

            return new Size(src.Width * scale, src.Height * scale);
        }

        private Size MeasureDevice()
        {
            if (Device == null)
                return Size.Empty;

            Rect rotationRect = new(0, 0, Device.RgbDevice.ActualSize.Width, Device.RgbDevice.ActualSize.Height);
            rotationRect.Transform(new RotateTransform(Device.Rotation).Value);

            return rotationRect.Size;
        }

        private void OnUnloaded(object? sender, RoutedEventArgs e)
        {
            _timer.Stop();

            if (_oldDevice != null)
            {
                if (Device != null)
                {
                    Device.RgbDevice.PropertyChanged -= DevicePropertyChanged;
                    Device.DeviceUpdated -= DeviceUpdated;
                }

                _oldDevice = null;
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Device == null)
                return;

            Point position = e.GetPosition(this);
            double x = position.X / RenderSize.Width;
            double y = position.Y / RenderSize.Height;

            Point scaledPosition = new(x * Device.Rectangle.Width, y * Device.Rectangle.Height);
            DeviceVisualizerLed? deviceVisualizerLed = _deviceVisualizerLeds.FirstOrDefault(l => l.DisplayGeometry != null && l.LedRect.Contains(scaledPosition));
            if (deviceVisualizerLed != null)
                OnLedClicked(new LedClickedEventArgs(deviceVisualizerLed.Led.Device, deviceVisualizerLed.Led));
        }

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            _timer.Start();
        }

        private void TimerOnTick(object? sender, EventArgs e)
        {
            Execute.PostToUIThread(() =>
            {
                if (ShowColors && Visibility == Visibility.Visible)
                    Render();
            });
        }

        private void UpdateTransform()
        {
            InvalidateVisual();
            InvalidateMeasure();
        }

        private static void DevicePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DeviceVisualizer deviceVisualizer = (DeviceVisualizer) d;
            deviceVisualizer.Dispatcher.Invoke(() => { deviceVisualizer.SetupForDevice(); });
        }

        private static void ShowColorsPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DeviceVisualizer deviceVisualizer = (DeviceVisualizer) d;
            deviceVisualizer.Dispatcher.Invoke(() => { deviceVisualizer.SetupForDevice(); });
        }

        private void SetupForDevice()
        {
            _deviceImage = null;
            _deviceVisualizerLeds.Clear();

            if (Device == null)
                return;

            if (_oldDevice != null)
            {
                Device.RgbDevice.PropertyChanged -= DevicePropertyChanged;
                Device.DeviceUpdated -= DeviceUpdated;
            }

            _oldDevice = Device;

            Device.RgbDevice.PropertyChanged += DevicePropertyChanged;
            Device.DeviceUpdated += DeviceUpdated;
            UpdateTransform();

            // Load the device main image
            if (Device.Layout?.Image != null && File.Exists(Device.Layout.Image.LocalPath))
                _deviceImage = new BitmapImage(Device.Layout.Image);

            // Create all the LEDs
            foreach (ArtemisLed artemisLed in Device.Leds)
                _deviceVisualizerLeds.Add(new DeviceVisualizerLed(artemisLed));

            if (!ShowColors)
            {
                InvalidateMeasure();
                return;
            }

            // Create the opacity drawing group
            DrawingGroup opacityDrawingGroup = new();
            DrawingContext drawingContext = opacityDrawingGroup.Open();
            foreach (DeviceVisualizerLed deviceVisualizerLed in _deviceVisualizerLeds)
                deviceVisualizerLed.RenderOpacityMask(drawingContext);
            drawingContext.Close();

            // Render the store as a bitmap 
            DrawingImage drawingImage = new(opacityDrawingGroup);
            Image image = new() {Source = drawingImage};
            RenderTargetBitmap bitmap = new(
                Math.Max(1, (int) (opacityDrawingGroup.Bounds.Width * 2.5)),
                Math.Max(1, (int) (opacityDrawingGroup.Bounds.Height * 2.5)),
                96,
                96,
                PixelFormats.Pbgra32
            );
            image.Arrange(new Rect(0, 0, bitmap.Width, bitmap.Height));
            bitmap.Render(image);
            bitmap.Freeze();

            // Set the bitmap as the opacity mask for the colors backing store
            ImageBrush bitmapBrush = new(bitmap);
            bitmapBrush.Freeze();
            _backingStore.OpacityMask = bitmapBrush;

            InvalidateMeasure();
        }

        private void DeviceUpdated(object? sender, EventArgs e)
        {
            Execute.PostToUIThread(SetupForDevice);
        }

        private void DevicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Execute.PostToUIThread(SetupForDevice);
        }

        private void Render()
        {
            DrawingContext drawingContext = _backingStore.Open();

            if (HighlightedLeds != null && HighlightedLeds.Any())
                foreach (DeviceVisualizerLed deviceVisualizerLed in _deviceVisualizerLeds)
                    deviceVisualizerLed.RenderColor(drawingContext, !HighlightedLeds.Contains(deviceVisualizerLed.Led));
            else
                foreach (DeviceVisualizerLed deviceVisualizerLed in _deviceVisualizerLeds)
                    deviceVisualizerLed.RenderColor(drawingContext, false);

            drawingContext.Close();
        }


        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}