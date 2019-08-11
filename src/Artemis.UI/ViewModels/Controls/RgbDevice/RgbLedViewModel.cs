using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using RGB.NET.Core;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;
using Stylet;
using Color = System.Windows.Media.Color;
using Shape = RGB.NET.Core.Shape;

namespace Artemis.UI.ViewModels.Controls.RgbDevice
{
    public class RgbLedViewModel : PropertyChangedBase
    {
        public RgbLedViewModel(Led led)
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

        public Color FillColor { get; set; }

        public DrawingImage DisplayDrawing { get; private set; }

        public string Tooltip => $"{Led.Id} - {Led.LedRectangle}";

        private void CreateLedGeometry()
        {
            var relativeRectangle = new Rect(0, 0, Led.LedRectangle.Width, Led.LedRectangle.Height);
            Geometry geometry;
            switch (Led.Shape)
            {
                case Shape.Custom:
                    geometry = Geometry.Parse(Led.ShapeData);
                    break;
                case Shape.Rectangle:
                    geometry = new RectangleGeometry(relativeRectangle, 2, 2);
                    break;
                case Shape.Circle:
                    geometry = new EllipseGeometry(relativeRectangle);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            var drawing = new GeometryDrawing(null, new Pen(null, 2), geometry);

            // The pen needs some adjustments when drawing custom shapes, a thickness of 2 just means you get a very thick pen that covers the
            // entire shape.. I'm not sure why to be honest sssh don't tell
            if (Led.Shape == Shape.Custom)
            {
                drawing.Pen.Thickness = 0.075;
                drawing.Pen.LineJoin = PenLineJoin.Round;
            }

            DisplayDrawing = new DrawingImage(drawing);
            NotifyOfPropertyChange(() => DisplayDrawing);
        }

        public void Update()
        {
            // Not leveraging on OnPropertyChanged since that'll update for each updated property
            var changed = false;

            var newFillColor = Color.FromRgb((byte) Math.Round(255 * Led.Color.R), (byte) Math.Round(255 * Led.Color.G), (byte) Math.Round(255 * Led.Color.B));
            if (!newFillColor.Equals(FillColor))
            {
                FillColor = newFillColor;
                changed = true;
            }

            if (Math.Abs(Led.LedRectangle.X - X) > 0.1)
            {
                X = Led.LedRectangle.X;
                changed = true;
            }

            if (Math.Abs(Led.LedRectangle.Y - Y) > 0.1)
            {
                Y = Led.LedRectangle.Y;
                changed = true;
            }

            if (Math.Abs(Led.LedRectangle.Width - Width) > 0.1)
            {
                Width = Led.LedRectangle.Width;
                changed = true;
            }

            if (Math.Abs(Led.LedRectangle.Height - Height) > 0.1)
            {
                Height = Led.LedRectangle.Height;
                changed = true;
            }

            if (DisplayDrawing != null && changed)
            {
                Execute.OnUIThread(() =>
                {
                    if (DisplayDrawing.Drawing is GeometryDrawing geometryDrawing)
                    {
                        geometryDrawing.Brush = new SolidColorBrush(FillColor) {Opacity = 0.3};
                        geometryDrawing.Pen.Brush = new SolidColorBrush(FillColor);
                    }

                    NotifyOfPropertyChange(() => DisplayDrawing);
                });
            }
        }
    }
}