using System;
using System.IO;
using Artemis.Core;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using RGB.NET.Core;
using Color = Avalonia.Media.Color;
using Point = Avalonia.Point;
using SolidColorBrush = Avalonia.Media.SolidColorBrush;

namespace Artemis.UI.Avalonia.Shared.Controls
{
    internal class DeviceVisualizerLed
    {
        private const byte Dimmed = 100;
        private const byte NonDimmed = 255;

        public DeviceVisualizerLed(ArtemisLed led)
        {
            Led = led;
            LedRect = new Rect(
                Led.RgbLed.Location.X,
                Led.RgbLed.Location.Y,
                Led.RgbLed.Size.Width,
                Led.RgbLed.Size.Height
            );

            if (Led.Layout?.Image != null && File.Exists(Led.Layout.Image.LocalPath))
                LedImage = new Bitmap(Led.Layout.Image.AbsolutePath);

            CreateLedGeometry();
        }

        public ArtemisLed Led { get; }
        public Rect LedRect { get; set; }
        public Bitmap? LedImage { get; set; }
        public Geometry? DisplayGeometry { get; private set; }
        
        public void RenderImage(DrawingContext drawingContext)
        {
            if (LedImage == null)
                return;

            drawingContext.DrawImage(LedImage, LedRect);

            byte r = Led.RgbLed.Color.GetR();
            byte g = Led.RgbLed.Color.GetG();
            byte b = Led.RgbLed.Color.GetB();
            SolidColorBrush fillBrush = new(new Color(100, r, g, b));
            SolidColorBrush penBrush = new(new Color(255, r, g, b));

            // Create transparent pixels covering the entire LedRect so the image size matched the LedRect size
            // drawingContext.DrawRectangle(new SolidColorBrush(Colors.Transparent), new Pen(new SolidColorBrush(Colors.Transparent), 1), LedRect);
            // Translate to the top-left of the LedRect
            using DrawingContext.PushedState push = drawingContext.PushPostTransform(Matrix.CreateTranslation(LedRect.X, LedRect.Y));
            // Render the LED geometry
            drawingContext.DrawGeometry(fillBrush, new Pen(penBrush) {LineJoin = PenLineJoin.Round}, DisplayGeometry);
        }

        public bool HitTest(Point position)
        {
            if (DisplayGeometry == null)
                return false;

            Geometry translatedGeometry = DisplayGeometry.Clone();
            translatedGeometry.Transform = new TranslateTransform(Led.RgbLed.Location.X, Led.RgbLed.Location.Y);
            return translatedGeometry.FillContains(position);
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
            PathGeometry path = PathGeometry.Parse($"M1,1" +
                                                   $"h{Led.RgbLed.Size.Width - 2} a10," +
                                                   $"10 0 0 1 10," +
                                                   $"10 v{Led.RgbLed.Size.Height - 2} a10," +
                                                   $"10 0 0 1 -10," +
                                                   $"10 h-{Led.RgbLed.Size.Width - 2} a10," +
                                                   $"10 0 0 1 -10," +
                                                   $"-10 v-{Led.RgbLed.Size.Height - 2} a10," +
                                                   $"10 0 0 1 10,-10 z");
            DisplayGeometry = path;
        }

        private void CreateCustomGeometry(double deflateAmount)
        {
            try
            {
                double width = Led.RgbLed.Size.Width - deflateAmount;
                double height = Led.RgbLed.Size.Height - deflateAmount;

                Geometry geometry = Geometry.Parse(Led.RgbLed.ShapeData);
                geometry.Transform = new ScaleTransform(width, height);
                geometry = geometry.Clone();
                geometry.Transform = new TranslateTransform(deflateAmount / 2, deflateAmount / 2);
                DisplayGeometry = geometry.Clone();

                // TODO: Figure out wtf was going on here
                // if (DisplayGeometry.Bounds.Width > width)
                // {
                //     DisplayGeometry = Geometry.Combine(Geometry.Empty, DisplayGeometry, GeometryCombineMode.Union, new TransformGroup
                //     {
                //         Children = new TransformCollection {new ScaleTransform(width / DisplayGeometry.Bounds.Width, 1)}
                //     });
                // }
                //
                // if (DisplayGeometry.Bounds.Height > height)
                // {
                //     DisplayGeometry = Geometry.Combine(Geometry.Empty, DisplayGeometry, GeometryCombineMode.Union, new TransformGroup
                //     {
                //         Children = new TransformCollection {new ScaleTransform(1, height / DisplayGeometry.Bounds.Height)}
                //     });
                // }
            }
            catch (Exception)
            {
                CreateRectangleGeometry();
            }
        }
    }
}