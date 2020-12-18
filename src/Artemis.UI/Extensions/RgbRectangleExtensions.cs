using System.Windows;
using RGB.NET.Core;

namespace Artemis.UI.Extensions
{
    public static class RgbRectangleExtensions
    {
        public static Rect ToWindowsRect(this Rectangle rectangle, double scale)
        {
            return new(
                (int) (rectangle.Location.X * scale),
                (int) (rectangle.Location.Y * scale),
                (int) (rectangle.Size.Width * scale),
                (int) (rectangle.Size.Height * scale)
            );
        }
    }
}