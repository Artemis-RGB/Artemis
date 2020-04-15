using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Artemis.Core.Models.Surface;
using RGB.NET.Core;
using Color = System.Windows.Media.Color;

namespace Artemis.UI.Shared.Controls
{
    internal class DeviceVisualizerLed
    {
        public DeviceVisualizerLed(ArtemisLed led)
        {
            Led = led;
            if (Led.RgbLed.Image != null && File.Exists(Led.RgbLed.Image.AbsolutePath))
                LedImage = new BitmapImage(Led.RgbLed.Image);
            CreateLedGeometry();
        }

        public ArtemisLed Led { get; }
        public BitmapImage LedImage { get; set; }

        public Geometry DisplayGeometry { get; private set; }

        internal void Render(DrawingContext drawingContext, bool renderGeometry)
        {
            if (!renderGeometry)
                RenderImage(drawingContext);
            else
                RenderGeometry(drawingContext);
        }

        private void CreateLedGeometry()
        {
            switch (Led.RgbLed.Shape)
            {
                case Shape.Custom:
                    if (Led.RgbLed.Device.DeviceInfo.DeviceType == RGBDeviceType.Keyboard || Led.RgbLed.Device.DeviceInfo.DeviceType == RGBDeviceType.Keypad)
                        CreateCustomGeometry(2.0);
                    else
                        CreateCustomGeometry(1.0);
                    break;
                case Shape.Rectangle:
                    if (Led.RgbLed.Device.DeviceInfo.DeviceType == RGBDeviceType.Keyboard || Led.RgbLed.Device.DeviceInfo.DeviceType == RGBDeviceType.Keypad)
                        CreateKeyCapGeometry();
                    else
                        CreateRectangleGeometry();
                    break;
                case Shape.Circle:
                    CreateCircleGeometry();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Stroke geometry is the display geometry excluding the inner geometry
            DisplayGeometry.Transform = new TranslateTransform(Led.RgbLed.LedRectangle.Location.X, Led.RgbLed.LedRectangle.Location.Y);
            // Try to gain some performance
            DisplayGeometry = DisplayGeometry.GetFlattenedPathGeometry();
            DisplayGeometry.Freeze();
        }

        private void CreateRectangleGeometry()
        {
            DisplayGeometry = new RectangleGeometry(new Rect(0.5, 0.5, Led.RgbLed.Size.Width - 1, Led.RgbLed.Size.Height - 1));
        }

        private void CreateCircleGeometry()
        {
            DisplayGeometry = new EllipseGeometry(new Rect(0.5, 0.5, Led.RgbLed.Size.Width - 1, Led.RgbLed.Size.Height - 1));
        }

        private void CreateKeyCapGeometry()
        {
            DisplayGeometry = new RectangleGeometry(new Rect(1, 1, Led.RgbLed.Size.Width - 2, Led.RgbLed.Size.Height - 2), 1.6, 1.6);
        }

        private void CreateCustomGeometry(double deflateAmount)
        {
            try
            {
                DisplayGeometry = Geometry.Combine(
                    Geometry.Empty,
                    Geometry.Parse(Led.RgbLed.ShapeData),
                    GeometryCombineMode.Union,
                    new TransformGroup
                    {
                        Children = new TransformCollection
                        {
                            new ScaleTransform(Led.RgbLed.Size.Width - deflateAmount, Led.RgbLed.Size.Height - deflateAmount),
                            new TranslateTransform(deflateAmount / 2, deflateAmount / 2)
                        }
                    }
                );
            }
            catch (Exception)
            {
                CreateRectangleGeometry();
            }
        }

        private void RenderGeometry(DrawingContext drawingContext)
        {
            if (DisplayGeometry == null)
                return;

            var r = Led.RgbLed.Color.GetR();
            var g = Led.RgbLed.Color.GetG();
            var b = Led.RgbLed.Color.GetB();
            
            var fillBrush = new SolidColorBrush(Color.FromArgb(100, r,g,b));
            fillBrush.Freeze();
            var penBrush = new SolidColorBrush(Color.FromArgb(255, r, g, b));
            penBrush.Freeze();
            
            drawingContext.DrawGeometry(fillBrush, new Pen(penBrush, 1), DisplayGeometry);
        }

        private void RenderImage(DrawingContext drawingContext)
        {
            if (LedImage == null)
                return;

            var ledRect = new Rect(
                Led.RgbLed.LedRectangle.Location.X,
                Led.RgbLed.LedRectangle.Location.Y,
                Led.RgbLed.LedRectangle.Size.Width,
                Led.RgbLed.LedRectangle.Size.Height
            );
            drawingContext.DrawImage(LedImage, ledRect);
        }
    }
}