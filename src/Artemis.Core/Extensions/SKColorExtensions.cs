using System;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core
{
    // ReSharper disable once InconsistentNaming - I didn't come up with SKColor
    public static class SKColorExtensions
    {
        public static Color ToRgbColor(this SKColor color)
        {
            return new Color(color.Alpha, color.Red, color.Green, color.Blue);
        }

        public static SKColor Interpolate(this SKColor from, SKColor to, float progress)
        {
            var redDiff = to.Red - from.Red;
            var greenDiff = to.Green - from.Green;
            var blueDiff = to.Blue - from.Blue;
            var alphaDiff = to.Alpha - from.Alpha;

            return new SKColor(
                ClampToByte(from.Red + redDiff * progress),
                ClampToByte(from.Green + greenDiff * progress),
                ClampToByte(from.Blue + blueDiff * progress),
                ClampToByte(from.Alpha + alphaDiff * progress)
            );
        }

        public static SKColor Sum(this SKColor a, SKColor b)
        {
            return new SKColor(
                ClampToByte(a.Red + b.Red),
                ClampToByte(a.Green + b.Green),
                ClampToByte(a.Blue + b.Blue),
                ClampToByte(a.Alpha + b.Alpha)
            );
        }

        private static byte ClampToByte(float value)
        {
            return (byte) Math.Clamp(value, 0, 255);
        }
    }
}