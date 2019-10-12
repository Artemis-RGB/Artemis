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
            
            Execute.OnUIThread(CreateLedGeometry);
            Update();
        }

        public Led Led { get; }

        public double X { get; private set; }
        public double Y { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }

        public Geometry DisplayGeometry { get; private set; }
        public Geometry StrokeGeometry { get; private set; }
        public Color DisplayColor { get; private set; }
        public bool ColorsEnabled { get; set; } = true;

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

            // Create a smaller version of the display geometry
            var innerGeometry = Geometry.Combine(
                Geometry.Empty,
                geometry,
                GeometryCombineMode.Union,
                new TransformGroup
                {
                    Children = new TransformCollection
                    {
                        new ScaleTransform(Led.LedRectangle.Width - 2, Led.LedRectangle.Height - 2),
                        new TranslateTransform(1, 1)
                    }
                }
            );
            // Stroke geometry is the display geometry excluding the inner geometry
            StrokeGeometry = Geometry.Combine(
                DisplayGeometry,
                innerGeometry,
                GeometryCombineMode.Exclude,
                null
            );
        }

        public void Update()
        {
            if (ColorsEnabled)
            {
                if (Led.Id == LedId.Keyboard_Y)
                    Console.WriteLine();
                var newColor = Led.Color.ToMediaColor();
                SetColor(newColor);
            }

            if (Math.Abs(Led.LedRectangle.X - X) > 0.1)
                X = Led.LedRectangle.X;

            if (Math.Abs(Led.LedRectangle.Y - Y) > 0.1)
                Y = Led.LedRectangle.Y;

            if (Math.Abs(Led.LedRectangle.Width - Width) > 0.1)
                Width = Led.LedRectangle.Width;

            if (Math.Abs(Led.LedRectangle.Height - Height) > 0.1)
                Height = Led.LedRectangle.Height;
        }

        public void SetColor(Color color)
        {
            if (!DisplayColor.Equals(color))
                DisplayColor = color;
        }
    }
}