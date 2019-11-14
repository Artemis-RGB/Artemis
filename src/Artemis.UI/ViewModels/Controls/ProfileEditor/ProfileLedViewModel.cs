using System;
using System.Windows;
using System.Windows.Media;
using Artemis.UI.Extensions;
using RGB.NET.Core;
using Stylet;
using Color = System.Windows.Media.Color;

namespace Artemis.UI.ViewModels.Controls.ProfileEditor
{
    public class ProfileLedViewModel : PropertyChangedBase
    {
        public ProfileLedViewModel(Led led)
        {
            Led = led;
            X = Led.LedRectangle.X;
            Y = Led.LedRectangle.Y;
            Width = Led.LedRectangle.Width;
            Height = Led.LedRectangle.Height;

            Execute.OnUIThread(CreateLedGeometry);
        }

        public Led Led { get; }

        public double X { get; private set; }
        public double Y { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }

        public Geometry DisplayGeometry { get; private set; }
        public Geometry StrokeGeometry { get; private set; }
        public Color DisplayColor { get; private set; }

        private void CreateLedGeometry()
        {
            switch (Led.Shape)
            {
                case Shape.Custom:
                    if (Led.Device.DeviceInfo.DeviceType == RGBDeviceType.Keyboard || Led.Device.DeviceInfo.DeviceType == RGBDeviceType.Keypad)
                        CreateCustomGeometry(2.0);
                    else
                        CreateCustomGeometry(1.0);
                    break;
                case Shape.Rectangle:
                    if (Led.Device.DeviceInfo.DeviceType == RGBDeviceType.Keyboard || Led.Device.DeviceInfo.DeviceType == RGBDeviceType.Keypad)
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
            StrokeGeometry = DisplayGeometry.GetWidenedPathGeometry(new Pen(null, 1.0), 0.1, ToleranceType.Absolute);
            DisplayGeometry.Freeze();
            StrokeGeometry.Freeze();
        }

        private void CreateRectangleGeometry()
        {
            DisplayGeometry = new RectangleGeometry(new Rect(0.5, 0.5, Led.LedRectangle.Width - 1, Led.LedRectangle.Height - 1));
        }

        private void CreateCircleGeometry()
        {
            DisplayGeometry = new EllipseGeometry(new Rect(0.5, 0.5, Led.LedRectangle.Width - 1, Led.LedRectangle.Height - 1));
        }

        private void CreateKeyCapGeometry()
        {
            DisplayGeometry = new RectangleGeometry(new Rect(1, 1, Led.LedRectangle.Width - 2, Led.LedRectangle.Height - 2), 1.6, 1.6);
        }

        private void CreateCustomGeometry(double deflateAmount)
        {
            try
            {
                DisplayGeometry = Geometry.Combine(
                    Geometry.Empty,
                    Geometry.Parse(Led.ShapeData),
                    GeometryCombineMode.Union,
                    new TransformGroup
                    {
                        Children = new TransformCollection
                        {
                            new ScaleTransform(Led.LedRectangle.Width - deflateAmount, Led.LedRectangle.Height - deflateAmount),
                            new TranslateTransform(deflateAmount/2, deflateAmount/2)
                        }
                    }
                );
            }
            catch (Exception)
            {
                CreateRectangleGeometry();
            }
        }

        public void Update()
        {
            var newColor = Led.Color.ToMediaColor();
            Execute.OnUIThread(() =>
            {
                if (!DisplayColor.Equals(newColor))
                    DisplayColor = newColor;
            });
        }
    }
}