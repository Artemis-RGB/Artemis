using System.Drawing;

namespace Artemis.Core.Extensions
{
    public static class RgbRectangleExtensions
    {
        public static Rectangle ToDrawingRectangle(this global::RGB.NET.Core.Rectangle rectangle)
        {
            return new Rectangle((int) rectangle.X, (int) rectangle.Y, (int) rectangle.Width, (int) rectangle.Height);
        }
    }
}