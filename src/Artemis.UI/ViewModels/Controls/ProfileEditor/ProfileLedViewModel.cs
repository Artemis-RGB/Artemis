using System;
using System.Windows;
using System.Windows.Media;
using Artemis.UI.Extensions;
using RGB.NET.Core;
using Stylet;

namespace Artemis.UI.ViewModels.Controls.ProfileEditor
{
    public class ProfileLedViewModel : PropertyChangedBase
    {
        public ProfileLedViewModel(Led led)
        {
            Led = led;

            Execute.OnUIThread(() =>
            {
                CreateLedGeometry();
                FillBrush = new SolidColorBrush { Opacity = 0.25 };
                BorderBrush = new SolidColorBrush();
            });
        }

        public Led Led { get; }

        public double X { get; private set; }
        public double Y { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }

        public Geometry DisplayGeometry { get; private set; }
        public SolidColorBrush FillBrush { get; set; }
        public SolidColorBrush BorderBrush { get; set; }


        public string Tooltip => $"{Led.Id} - {Led.LedRectangle}";

        private void CreateLedGeometry()
        {
            var geometryRectangle = new Rect(0, 0, 1, 1);
            Geometry geometry;

            switch (Led.Shape)
            {
                case Shape.Custom:
                    try
                    {
                        geometry = Geometry.Parse(Led.ShapeData);
                    }
                    catch (Exception)
                    {
                        geometry = new RectangleGeometry(geometryRectangle);
                    }

                    break;
                case Shape.Rectangle:
                    geometry = new RectangleGeometry(geometryRectangle);
                    break;
                case Shape.Circle:
                    geometry = new EllipseGeometry(geometryRectangle);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            DisplayGeometry = Geometry.Combine(
                Geometry.Empty,
                geometry,
                GeometryCombineMode.Union,
                new ScaleTransform(Led.LedRectangle.Width, Led.LedRectangle.Height)
            );
        }

        public void Update()
        {
            var newColor = Led.Color.ToMediaColor();
            Execute.OnUIThread(() =>
            {
                if (!FillBrush.Color.Equals(newColor))
                {
                    FillBrush.Color = newColor;
                    BorderBrush.Color = newColor;
                }
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