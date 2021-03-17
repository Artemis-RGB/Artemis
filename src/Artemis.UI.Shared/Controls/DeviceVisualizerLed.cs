using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Artemis.Core;
using RGB.NET.Core;
using Color = System.Windows.Media.Color;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;

namespace Artemis.UI.Shared
{
    internal class DeviceVisualizerLed
    {
        private const byte Dimmed = 100;
        private const byte NonDimmed = 255;

        private SolidColorBrush? _renderColorBrush;
        private Color _renderColor;

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
                LedImage = new BitmapImage(Led.Layout.Image);

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

            _renderColorBrush ??= new SolidColorBrush();

            byte r = Led.RgbLed.Color.GetR();
            byte g = Led.RgbLed.Color.GetG();
            byte b = Led.RgbLed.Color.GetB();

            _renderColor.A = isDimmed ? Dimmed : NonDimmed;
            _renderColor.R = r;
            _renderColor.G = g;
            _renderColor.B = b;
            _renderColorBrush.Color = _renderColor;
            drawingContext.DrawRectangle(_renderColorBrush, null, LedRect);
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

            SolidColorBrush fillBrush = new(Color.FromArgb(100, 255, 255, 255));
            fillBrush.Freeze();
            SolidColorBrush penBrush = new(Color.FromArgb(255, 255, 255, 255));
            penBrush.Freeze();

            // Create transparent pixels covering the entire LedRect so the image size matched the LedRect size
            drawingContext.DrawRectangle(new SolidColorBrush(Colors.Transparent), new Pen(new SolidColorBrush(Colors.Transparent), 1), LedRect);
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
                double width = Led.RgbLed.Size.Width - deflateAmount;
                double height = Led.RgbLed.Size.Height - deflateAmount;
                // DisplayGeometry = Geometry.Parse(Led.RgbLed.ShapeData);
                DisplayGeometry = Geometry.Combine(
                    Geometry.Empty,
                    Geometry.Parse(Led.RgbLed.ShapeData),
                    GeometryCombineMode.Union,
                    new TransformGroup
                    {
                        Children = new TransformCollection
                        {
                            new ScaleTransform(width, height),
                            new TranslateTransform(deflateAmount / 2, deflateAmount / 2)
                        }
                    }
                );

                if (DisplayGeometry.Bounds.Width > width)
                {
                    DisplayGeometry = Geometry.Combine(Geometry.Empty, DisplayGeometry, GeometryCombineMode.Union, new TransformGroup
                    {
                        Children = new TransformCollection {new ScaleTransform(width / DisplayGeometry.Bounds.Width, 1)}
                    });
                }

                if (DisplayGeometry.Bounds.Height > height)
                {
                    DisplayGeometry = Geometry.Combine(Geometry.Empty, DisplayGeometry, GeometryCombineMode.Union, new TransformGroup
                    {
                        Children = new TransformCollection {new ScaleTransform(1, height / DisplayGeometry.Bounds.Height)}
                    });
                }
            }
            catch (Exception)
            {
                CreateRectangleGeometry();
            }
        }
    }
}