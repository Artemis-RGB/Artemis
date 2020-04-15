using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Artemis.Core.Models.Surface;
using RGB.NET.Core;
using Stylet;

namespace Artemis.UI.Shared.Controls
{
    public class DeviceVisualizer : FrameworkElement
    {
        public static readonly DependencyProperty DeviceProperty = DependencyProperty.Register(nameof(Device), typeof(ArtemisDevice), typeof(DeviceVisualizer),
            new FrameworkPropertyMetadata(default(ArtemisDevice), FrameworkPropertyMetadataOptions.AffectsRender, DevicePropertyChangedCallback));

        public static readonly DependencyProperty ShowColorsProperty = DependencyProperty.Register(nameof(ShowColors), typeof(bool), typeof(DeviceVisualizer),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender));

        private BitmapImage _deviceImage;
        private List<DeviceVisualizerLed> _deviceVisualizerLeds;
        private DrawingGroup _backingStore;

        private static void DevicePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var deviceVisualizer = (DeviceVisualizer) d;
            deviceVisualizer.Dispatcher.Invoke(() =>
            {
                deviceVisualizer.SubscribeToSurfaceUpdate((ArtemisDevice) e.OldValue, (ArtemisDevice) e.NewValue);
                deviceVisualizer.Initialize();
            });
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

        public DeviceVisualizer()
        {
            _backingStore = new DrawingGroup();
            _deviceVisualizerLeds = new List<DeviceVisualizerLed>();
        }


        private void Initialize()
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
            {
                _deviceVisualizerLeds.Add(new DeviceVisualizerLed(artemisLed));
            }
        }

        private void SubscribeToSurfaceUpdate(ArtemisDevice oldValue, ArtemisDevice newValue)
        {
            if (oldValue != null)
                oldValue.Surface.RgbSurface.Updated -= RgbSurfaceOnUpdated;
            if (newValue != null)
                newValue.Surface.RgbSurface.Updated += RgbSurfaceOnUpdated;
        }

        private void RgbSurfaceOnUpdated(UpdatedEventArgs args)
        {
            Dispatcher.Invoke(() =>
            {
                if (ShowColors)
                {
                    Render();
                }
            });
        }

        private void Render()
        {
            var drawingContext = _backingStore.Open();

            foreach (var deviceVisualizerLed in _deviceVisualizerLeds)
                deviceVisualizerLed.Render(drawingContext, true);

            drawingContext.Close();
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
                deviceVisualizerLed.Render(drawingContext, false);

            drawingContext.DrawDrawing(_backingStore);
        }
    }
}