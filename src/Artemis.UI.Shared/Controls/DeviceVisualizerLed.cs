using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Artemis.Core;
using RGB.NET.Core;
using Color = System.Windows.Media.Color;

namespace Artemis.UI.Shared
{
    internal class DeviceVisualizerLed
    {
        public DeviceVisualizerLed(ArtemisLed led)
        {
            Led = led;
            LedRect = new Rect(
                Led.RgbLed.Location.X,
                Led.RgbLed.Location.Y,
                Led.RgbLed.Size.Width,
                Led.RgbLed.Size.Height
            );

            if (Led.RgbLed.Image != null && File.Exists(Led.RgbLed.Image.AbsolutePath))
                LedImage = new BitmapImage(Led.RgbLed.Image);

            CreateLedGeometry();
        }


        public ArtemisLed Led { get; }
        public Rect LedRect { get; set; }
        public BitmapImage? LedImage { get; set; }
        public Geometry? DisplayGeometry { get; private set; }

        public void RenderColor(DrawingContext drawingContext, bool isDimmed)
        {
            if (DisplayGeometry == null)
                return;

            byte r = Led.RgbLed.Color.GetR();
            byte g = Led.RgbLed.Color.GetG();
            byte b = Led.RgbLed.Color.GetB();

            drawingContext.DrawRectangle(isDimmed
                ? new SolidColorBrush(Color.FromArgb(100, r, g, b))
                : new SolidColorBrush(Color.FromRgb(r, g, b)), null, LedRect);
        }

        public void RenderImage(DrawingContext drawingContext)
        {
            if (LedImage == null)
                return;

            drawingContext.DrawImage(LedImage, LedRect);
        }

        public void RenderOpacityMask(DrawingContext drawingContext)
        {
            if (DisplayGeometry == null)
                return;

            SolidColorBrush fillBrush = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
            fillBrush.Freeze();
            SolidColorBrush penBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            penBrush.Freeze();

            // Create transparent pixels covering the entire LedRect so the image size matched the LedRect size
            drawingContext.DrawRectangle(new SolidColorBrush(Colors.Transparent), null, LedRect);
            // Translate to the top-left of the LedRect
            drawingContext.PushTransform(new TranslateTransform(LedRect.X, LedRect.Y));
            // Render the LED geometry
            drawingContext.DrawGeometry(fillBrush, new Pen(penBrush, 1) {LineJoin = PenLineJoin.Round}, DisplayGeometry.GetOutlinedPathGeometry());
            // Restore the drawing context
            drawingContext.Pop();
        }

        private void CreateLedGeometry()
        {
            // The minimum required size for geometry to be created
            if (Led.RgbLed.Size.Width < 2 || Led.RgbLed.Size.Height < 2)
                return;

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
                // DisplayGeometry = Geometry.Parse(Led.RgbLed.ShapeData);
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
    }
}