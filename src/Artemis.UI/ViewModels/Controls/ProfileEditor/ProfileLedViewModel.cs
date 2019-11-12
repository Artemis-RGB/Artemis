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

            Execute.OnUIThread(() => { CreateLedGeometry(); });
        }

        public Led Led { get; }

        public double X { get; private set; }
        public double Y { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }

        public Geometry DisplayGeometry { get; private set; }
        public Geometry StrokeGeometry { get; private set; }
        public Color DisplayColor { get; private set; }

        public string Tooltip => $"{Led.Id} - {Led.LedRectangle}";

        private void CreateLedGeometry()
        {
            switch (Led.Shape)
            {
                case Shape.Custom:
                    CreateCustomGeometry();
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

        private void CreateCustomGeometry()
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
                            new ScaleTransform(Led.LedRectangle.Width - 1, Led.LedRectangle.Height - 1),
                            new TranslateTransform(0.5, 0.5)
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

            if (Math.Abs(Led.LedRectangle.X - X) > 0.1)
                X = Led.LedRectangle.X;

            if (Math.Abs(Led.LedRectangle.Y - Y) > 0.1)
                Y = Led.LedRectangle.Y;

            if (Math.Abs(Led.LedRectangle.Width - Width) > 0.1)
                Width = Led.LedRectangle.Width;

            if (Math.Abs(Led.LedRectangle.Height - Height) > 0.1)
                Height = Led.LedRectangle.Height;
        }
    }
}