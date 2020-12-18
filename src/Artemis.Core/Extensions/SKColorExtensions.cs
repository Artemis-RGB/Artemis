using System;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core
{
    /// <summary>
    ///     A static class providing <see cref="SKColor" /> extensions
    /// </summary>
    public static class SKColorExtensions
    {
        /// <summary>
        ///     Converts hte SKColor to an RGB.NET color
        /// </summary>
        /// <param name="color">The color to convert</param>
        /// <returns>The RGB.NET color</returns>
        public static Color ToRgbColor(this SKColor color)
        {
            return new(color.Alpha, color.Red, color.Green, color.Blue);
        }

        /// <summary>
        ///     Interpolates a color between the <paramref name="from" /> and <paramref name="to" /> color.
        /// </summary>
        /// <param name="from">The first color</param>
        /// <param name="to">The second color</param>
        /// <param name="progress">A value between 0 and 1</param>
        /// <returns>The interpolated color</returns>
        public static SKColor Interpolate(this SKColor from, SKColor to, float progress)
        {
            int redDiff = to.Red - from.Red;
            int greenDiff = to.Green - from.Green;
            int blueDiff = to.Blue - from.Blue;
            int alphaDiff = to.Alpha - from.Alpha;

            return new SKColor(
                ClampToByte(from.Red + redDiff * progress),
                ClampToByte(from.Green + greenDiff * progress),
                ClampToByte(from.Blue + blueDiff * progress),
                ClampToByte(from.Alpha + alphaDiff * progress)
            );
        }

        /// <summary>
        ///     Adds the two colors together
        /// </summary>
        /// <param name="a">The first color</param>
        /// <param name="b">The second color</param>
        /// <returns>The sum of the two colors</returns>
        public static SKColor Sum(this SKColor a, SKColor b)
        {
            return new(
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