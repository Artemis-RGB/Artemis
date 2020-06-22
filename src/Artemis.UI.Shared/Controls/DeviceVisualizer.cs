using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Artemis.Core.Models.Surface;
using RGB.NET.Core;
using Size = System.Windows.Size;

namespace Artemis.UI.Shared.Controls
{
    public class DeviceVisualizer : FrameworkElement, IDisposable
    {
        public static readonly DependencyProperty DeviceProperty = DependencyProperty.Register(nameof(Device), typeof(ArtemisDevice), typeof(DeviceVisualizer),
            new FrameworkPropertyMetadata(default(ArtemisDevice), FrameworkPropertyMetadataOptions.AffectsRender, DevicePropertyChangedCallback));

        public static readonly DependencyProperty ShowColorsProperty = DependencyProperty.Register(nameof(ShowColors), typeof(bool), typeof(DeviceVisualizer),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender, ShowColorsPropertyChangedCallback));

        public static readonly DependencyProperty HighlightedLedsProperty = DependencyProperty.Register(nameof(HighlightedLeds), typeof(IEnumerable<ArtemisLed>), typeof(DeviceVisualizer),
            new FrameworkPropertyMetadata(default(IEnumerable<ArtemisLed>)));

        private readonly DrawingGroup _backingStore;
        private readonly List<DeviceVisualizerLed> _deviceVisualizerLeds;
        private BitmapImage _deviceImage;
        private bool _subscribed;
        private ArtemisDevice _oldDevice;

        public DeviceVisualizer()
        {
            _backingStore = new DrawingGroup();
            _deviceVisualizerLeds = new List<DeviceVisualizerLed>();

            Loaded += (sender, args) => SubscribeToUpdate(true);
            Unloaded += (sender, args) => SubscribeToUpdate(false);
        }

        public ArtemisDevice Device
        {
            get => (ArtemisDevice) GetValue(DeviceProperty);
            set => SetValue(DeviceProperty, value);
        }

        public bool ShowColors
        {
            get => (bool) GetValue(ShowColorsProperty);
            set => SetValue(ShowColorsProperty, value);
        }

        public IEnumerable<ArtemisLed> HighlightedLeds
        {
            get => (IEnumerable<ArtemisLed>) GetValue(HighlightedLedsProperty);
            set => SetValue(HighlightedLedsProperty, value);
        }

        public void Dispose()
        {
            RGBSurface.Instance.Updated -= RgbSurfaceOnUpdated;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (Device == null)
                return;

            // Determine the scale required to fit the desired size of the control
            var measureSize = MeasureOverride(Size.Empty);
            var scale = Math.Min(DesiredSize.Width / measureSize.Width, DesiredSize.Height / measureSize.Height);
            var scaledRect = new Rect(0, 0, measureSize.Width * scale, measureSize.Height * scale);
            
            // Center and scale the visualization in the desired bounding box
            if (DesiredSize.Width > 0 && DesiredSize.Height > 0)
            {
                drawingContext.PushTransform(new TranslateTransform(DesiredSize.Width / 2 - scaledRect.Width / 2, DesiredSize.Height / 2 - scaledRect.Height / 2));
                drawingContext.PushTransform(new ScaleTransform(scale, scale));
            }

            // Determine the offset required to rotate within bounds
            var rotationRect = new Rect(0, 0, Device.RgbDevice.ActualSize.Width, Device.RgbDevice.ActualSize.Height);
            rotationRect.Transform(new RotateTransform(Device.Rotation).Value);

            // Apply device rotation
            drawingContext.PushTransform(new TranslateTransform(0 - rotationRect.Left, 0 - rotationRect.Top));
            drawingContext.PushTransform(new RotateTransform(Device.Rotation));

            // Apply device scale
            drawingContext.PushTransform(new ScaleTransform(Device.Scale, Device.Scale));

            // Render device and LED images 
            if (_deviceImage != null)
                drawingContext.DrawImage(_deviceImage, new Rect(0, 0, Device.RgbDevice.Size.Width, Device.RgbDevice.Size.Height));
            
            foreach (var deviceVisualizerLed in _deviceVisualizerLeds)
                deviceVisualizerLed.RenderImage(drawingContext);

            drawingContext.DrawDrawing(_backingStore);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (Device == null)
                return Size.Empty;

            var rotationRect = new Rect(0, 0, Device.RgbDevice.ActualSize.Width, Device.RgbDevice.ActualSize.Height);
            rotationRect.Transform(new RotateTransform(Device.Rotation).Value);

            return rotationRect.Size;
        }


        private void UpdateTransform()
        {
            InvalidateVisual();
            InvalidateMeasure();
        }

        private void SubscribeToUpdate(bool subscribe)
        {
            if (_subscribed == subscribe)
                return;

            if (subscribe)
                RGBSurface.Instance.Updated += RgbSurfaceOnUpdated;
            else
                RGBSurface.Instance.Updated -= RgbSurfaceOnUpdated;

            _subscribed = subscribe;
        }

        private static void DevicePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var deviceVisualizer = (DeviceVisualizer) d;
            deviceVisualizer.Dispatcher.Invoke(() => { deviceVisualizer.SetupForDevice(); });
        }

        private static void ShowColorsPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var deviceVisualizer = (DeviceVisualizer) d;
            deviceVisualizer.Dispatcher.Invoke(() => { deviceVisualizer.SetupForDevice(); });
        }

        private void SetupForDevice()
        {
            _deviceImage = null;
            _deviceVisualizerLeds.Clear();

            if (Device == null)
                return;

            if (_oldDevice != null)
                Device.RgbDevice.PropertyChanged -= DevicePropertyChanged;
            _oldDevice = Device;

            Device.RgbDevice.PropertyChanged += DevicePropertyChanged;
            UpdateTransform();

            // Load the device main image
            if (Device.RgbDevice?.DeviceInfo?.Image?.AbsolutePath != null && File.Exists(Device.RgbDevice.DeviceInfo.Image.AbsolutePath))
                _deviceImage = new BitmapImage(Device.RgbDevice.DeviceInfo.Image);

            // Create all the LEDs
            foreach (var artemisLed in Device.Leds)
                _deviceVisualizerLeds.Add(new DeviceVisualizerLed(artemisLed));

            if (!ShowColors)
            {
                InvalidateMeasure();
                return;
            }

            // Create the opacity drawing group
            var opacityDrawingGroup = new DrawingGroup();
            var drawingContext = opacityDrawingGroup.Open();
            foreach (var deviceVisualizerLed in _deviceVisualizerLeds)
                deviceVisualizerLed.RenderOpacityMask(drawingContext);
            drawingContext.Close();

            // Render the store as a bitmap 
            var drawingImage = new DrawingImage(opacityDrawingGroup);
            var image = new Image {Source = drawingImage};
            var bitmap = new RenderTargetBitmap(
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
            var bitmapBrush = new ImageBrush(bitmap);
            bitmapBrush.Freeze();
            _backingStore.OpacityMask = bitmapBrush;

            InvalidateMeasure();
        }

        private void DevicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Device.RgbDevice.Scale) || e.PropertyName == nameof(Device.RgbDevice.Rotation))
                UpdateTransform();
        }

        private void RgbSurfaceOnUpdated(UpdatedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (ShowColors && Visibility == Visibility.Visible)
                    Render();
            });
        }

        private void Render()
        {
            var drawingContext = _backingStore.Open();

            if (HighlightedLeds != null && HighlightedLeds.Any())
            {
                foreach (var deviceVisualizerLed in _deviceVisualizerLeds)
                    deviceVisualizerLed.RenderColor(drawingContext, !HighlightedLeds.Contains(deviceVisualizerLed.Led));
            }
            else
            {
                foreach (var deviceVisualizerLed in _deviceVisualizerLeds)
                    deviceVisualizerLed.RenderColor(drawingContext, false);
            }

            drawingContext.Close();
        }
    }
}