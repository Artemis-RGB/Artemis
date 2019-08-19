using RGB.NET.Core;
using Color = System.Windows.Media.Color;

namespace Artemis.Core.Extensions
{
    public static class RgbColorExtensions
    {
        public static Color ToMediaColor(this global::RGB.NET.Core.Color color)
        {
            var (_, r, g, b) = color.GetRGBBytes();
            return Color.FromRgb(r, g, b);
        }
    }
}