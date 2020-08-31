using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core
{
    public static class RgbRectangleExtensions
    {
        public static SKRect ToSKRect(this Rectangle rectangle, double scale)
        {
            return SKRect.Create(
                (rectangle.Location.X * scale).RoundToInt(),
                (rectangle.Location.Y * scale).RoundToInt(),
                (rectangle.Size.Width * scale).RoundToInt(),
                (rectangle.Size.Height * scale).RoundToInt()
            );
        }
    }
}