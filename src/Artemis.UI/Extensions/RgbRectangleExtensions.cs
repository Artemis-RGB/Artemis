using System.Drawing;

namespace Artemis.UI.Extensions
{
    public static class RgbRectangleExtensions
    {
        public static Rectangle ToDrawingRectangle(this RGB.NET.Core.Rectangle rectangle)
        {
            return new Rectangle((int) rectangle.X, (int) rectangle.Y, (int) rectangle.Width, (int) rectangle.Height);
        }
    }
}