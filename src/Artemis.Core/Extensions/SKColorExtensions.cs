using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.Extensions
{
    // ReSharper disable once InconsistentNaming - I didn't come up with SKColor
    public static class SKColorExtensions
    {
        public static Color ToRgbColor(this SKColor color)
        {
            return new Color(color.Alpha, color.Red, color.Green, color.Blue);
        }
    }
}