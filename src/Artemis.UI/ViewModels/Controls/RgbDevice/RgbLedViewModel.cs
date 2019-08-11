using System;
using System.Windows;
using System.Windows.Media;
using RGB.NET.Core;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;
using Stylet;
using Color = System.Windows.Media.Color;

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
            // Prepare the SVG for this LED
            var converter = new FileSvgReader(new WpfDrawingSettings {OptimizePath = true});
            if (Led.Image != null)
            {
                DisplayDrawing = new DrawingImage(converter.Read(Led.Image));
            }
            else
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
                DisplayDrawing = new DrawingImage(drawing);
            }

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
                        geometryDrawing.Brush = new SolidColorBrush(FillColor) {Opacity = 0.6};
                        geometryDrawing.Pen.Brush = new SolidColorBrush(FillColor);
                    }
                    else if (DisplayDrawing.Drawing is DrawingGroup drawingGroup)
                        drawingGroup.OpacityMask = new SolidColorBrush(FillColor);
                });
                NotifyOfPropertyChange(() => DisplayDrawing);
            }
        }
    }
}