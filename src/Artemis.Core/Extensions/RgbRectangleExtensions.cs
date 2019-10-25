using System.Drawing;

namespace Artemis.Core.Extensions
{
    public static class RgbRectangleExtensions
    {
        public static Rectangle ToDrawingRectangle(this global::RGB.NET.Core.Rectangle rectangle, double scale)
        {
            return new Rectangle(
                (int) (rectangle.X * scale),
                (int) (rectangle.Y * scale),
                (int) (rectangle.Width * scale),
                (int) (rectangle.Height * scale)
            );
        }
    }
}