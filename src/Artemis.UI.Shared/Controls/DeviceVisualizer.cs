using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Artemis.Core.Models.Surface;
using RGB.NET.Core;

namespace Artemis.UI.Shared.Controls
{
    public class DeviceVisualizer : FrameworkElement, IDisposable
    {
        public static readonly DependencyProperty DeviceProperty = DependencyProperty.Register(nameof(Device), typeof(ArtemisDevice), typeof(DeviceVisualizer),
            new FrameworkPropertyMetadata(default(ArtemisDevice), FrameworkPropertyMetadataOptions.AffectsRender, DevicePropertyChangedCallback));

        public static readonly DependencyProperty ShowColorsProperty = DependencyProperty.Register(nameof(ShowColors), typeof(bool), typeof(DeviceVisualizer),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender, ShowColorsPropertyChangedCallback));

        private readonly DrawingGroup _backingStore;
        private readonly List<DeviceVisualizerLed> _deviceVisualizerLeds;
        private BitmapImage _deviceImage;

        public DeviceVisualizer()
        {
            _backingStore = new DrawingGroup();
            _deviceVisualizerLeds = new List<DeviceVisualizerLed>();

            RGBSurface.Instance.Updated += RgbSurfaceOnUpdated;
            Unloaded += (sender, args) => Dispose();
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

        public void Dispose()
        {
            RGBSurface.Instance.Updated -= RgbSurfaceOnUpdated;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (Device == null)
                return;

            // Determine the scale required to fit the desired size of the control
            var scale = Math.Min(DesiredSize.Width / Device.RgbDevice.Size.Width, DesiredSize.Height / Device.RgbDevice.Size.Height);
            var scaledRect = new Rect(0, 0, Device.RgbDevice.Size.Width * scale, Device.RgbDevice.Size.Height * scale);

            // Center and scale the visualization in the desired bounding box
            if (DesiredSize.Width > 0 && DesiredSize.Height > 0)
            {
                drawingContext.PushTransform(new TranslateTransform(DesiredSize.Width / 2 - scaledRect.Width / 2, DesiredSize.Height / 2 - scaledRect.Height / 2));
                drawingContext.PushTransform(new ScaleTransform(scale, scale));
            }

            // Render device and LED images 
            if (_deviceImage != null)
                drawingContext.DrawImage(_deviceImage, new Rect(0, 0, Device.RgbDevice.Size.Width, Device.RgbDevice.Size.Height));

            foreach (var deviceVisualizerLed in _deviceVisualizerLeds)
                deviceVisualizerLed.RenderImage(drawingContext);

            drawingContext.DrawDrawing(_backingStore);
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

            // Load the device main image
            if (Device.RgbDevice?.DeviceInfo?.Image?.AbsolutePath != null && File.Exists(Device.RgbDevice.DeviceInfo.Image.AbsolutePath))
                _deviceImage = new BitmapImage(Device.RgbDevice.DeviceInfo.Image);

            // Create all the LEDs
            foreach (var artemisLed in Device.Leds)
                _deviceVisualizerLeds.Add(new DeviceVisualizerLed(artemisLed));

            if (!ShowColors)
                return;

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
                (int) (opacityDrawingGroup.Bounds.Width * 2.5),
                (int) (opacityDrawingGroup.Bounds.Height * 2.5),
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

            foreach (var deviceVisualizerLed in _deviceVisualizerLeds)
                deviceVisualizerLed.RenderColor(drawingContext);

            drawingContext.Close();
        }
    }
}