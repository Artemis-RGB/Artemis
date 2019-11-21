using System.Drawing;

namespace Artemis.Core.Extensions
{
    public static class RgbRectangleExtensions
    {
        public static Rectangle ToDrawingRectangle(this global::RGB.NET.Core.Rectangle rectangle, double scale)
        {
            return new Rectangle(
                (int) (rectangle.Location.X * scale),
                (int) (rectangle.Location.Y * scale),
                (int) (rectangle.Size.Width * scale),
                (int) (rectangle.Size.Height * scale)
            );
        }
    }
}