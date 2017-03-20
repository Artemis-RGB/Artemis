using System.Windows;
using System.Windows.Media;
using Artemis.Utilities;
using Color = System.Drawing.Color;
using Point = System.Windows.Point;

namespace Artemis.Modules.General.Bubbles
{
    public class Bubble
    {
        #region Constructors

        public Bubble(Color color, int radius, Point position, Vector direction)
        {
            Color = color;
            Radius = radius;
            Position = position;
            Direction = direction;
        }

        #endregion

        #region Properties & Fields

        private SolidColorBrush _brush;

        private Color _color;

        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                _brush = new SolidColorBrush(ColorHelpers.ToMediaColor(_color));
            }
        }

        public int Radius { get; set; }
        public Point Position { get; private set; }
        public Vector Direction { get; private set; }

        #endregion

        #region Methods

        public void CheckCollision(Rect border)
        {
            if (Position.X - Radius < border.X || Position.X + Radius > border.X + border.Width)
                Direction = new Vector(Direction.X * -1, Direction.Y);

            if (Position.Y - Radius < border.Y || Position.Y + Radius > border.Y + border.Height)
                Direction = new Vector(Direction.X, Direction.Y * -1);
        }

        public void Move()
        {
            Position += Direction;
        }

        public void Draw(DrawingContext drawingContext)
        {
            drawingContext.DrawEllipse(_brush, new Pen(_brush, 1), new Point((float) Position.X - Radius, (float) Position.Y - Radius), Radius * 2, Radius * 2);
        }

        #endregion
    }
}
