using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Artemis.UI.Adorners
{
    public class BoundingBoxAdorner : Adorner
    {
        private Color _boundingBoxColor;
        private Rect _boundingBoxRect;

        public BoundingBoxAdorner(UIElement adornedElement, Color boundingBoxColor) : base(adornedElement)
        {
            _boundingBoxRect = new Rect(new Size(0, 0));
            BoundingBoxColor = boundingBoxColor;

            IsHitTestVisible = false;
        }

        public Color BoundingBoxColor
        {
            get => _boundingBoxColor;
            set
            {
                _boundingBoxColor = value;
                InvalidateVisual();
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var renderBrush = new SolidColorBrush(BoundingBoxColor) {Opacity = 0.2};
            var renderPen = new Pen(new SolidColorBrush(BoundingBoxColor), 1.5);

            drawingContext.DrawRectangle(renderBrush, renderPen, _boundingBoxRect);
        }

        public void Update(Point startingPoint, Point currentPoint)
        {
            _boundingBoxRect = new Rect(startingPoint, currentPoint);
            InvalidateVisual();
        }
    }
}