using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Color = System.Drawing.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace Artemis.Utilities
{
    public static class ColorHelpers
    {
        private static readonly Random _rand = new Random();

        /// <summary>
        ///     Comes up with a 'pure' psuedo-random color
        /// </summary>
        /// <returns>The color</returns>
        public static Color GetRandomRainbowColor()
        {
            var colors = new List<int>();
            for (var i = 0; i < 3; i++)
                colors.Add(_rand.Next(0, 256));

            var highest = colors.Max();
            var lowest = colors.Min();
            colors[colors.FindIndex(c => c == highest)] = 255;
            colors[colors.FindIndex(c => c == lowest)] = 0;

            var returnColor = Color.FromArgb(255, colors[0], colors[1], colors[2]);

            return returnColor;
        }

        public static System.Windows.Media.Color GetRandomRainbowMediaColor()
        {
            var colors = new List<byte>();
            for (var i = 0; i < 3; i++)
                colors.Add((byte) _rand.Next(0, 256));

            var highest = colors.Max();
            var lowest = colors.Min();
            colors[colors.FindIndex(c => c == highest)] = 255;
            colors[colors.FindIndex(c => c == lowest)] = 0;

            var returnColor = System.Windows.Media.Color.FromArgb(255, colors[0], colors[1], colors[2]);

            return returnColor;
        }

        public static System.Windows.Media.Color FromHex(this System.Windows.Media.Color c, string hex)
        {
            if (hex == null)
                return new System.Windows.Media.Color();

            var convertFromString = ColorConverter.ConvertFromString(hex);
            if (convertFromString != null)
                return (System.Windows.Media.Color) convertFromString;

            throw new ArgumentException("Invalid hex color code");
        }

        public static string ToHex(this System.Windows.Media.Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        public static Color ShiftColor(Color c, int shiftAmount)
        {
            int newRed = c.R;
            int newGreen = c.G;
            int newBlue = c.B;

            // Red to purple
            if (c.R == 255 && c.B < 255 && c.G == 0)
                newBlue = newBlue + shiftAmount;
            // Purple to blue
            else if (c.B == 255 && c.R > 0)
                newRed = newRed - shiftAmount;
            // Blue to light-blue
            else if (c.B == 255 && c.G < 255)
                newGreen = newGreen + shiftAmount;
            // Light-blue to green
            else if (c.G == 255 && c.B > 0)
                newBlue = newBlue - shiftAmount;
            // Green to yellow
            else if (c.G == 255 && c.R < 255)
                newRed = newRed + shiftAmount;
            // Yellow to red
            else if (c.R == 255 && c.G > 0)
                newGreen = newGreen - shiftAmount;

            newRed = BringIntInColorRange(newRed);
            newGreen = BringIntInColorRange(newGreen);
            newBlue = BringIntInColorRange(newBlue);

            return Color.FromArgb(c.A, newRed, newGreen, newBlue);
        }

        private static int BringIntInColorRange(int i)
        {
            if (i < 0)
                return 0;
            if (i > 255)
                return 255;

            return i;
        }

        public static Color ToDrawingColor(System.Windows.Media.Color mColor)
        {
            return Color.FromArgb(mColor.A, mColor.R, mColor.G, mColor.B);
        }

        public static System.Windows.Media.Color ToMediaColor(Color dColor)
        {
            return System.Windows.Media.Color.FromArgb(dColor.A, dColor.R, dColor.G, dColor.B);
        }

        public static Brush RandomizeBrush(Brush brush)
        {
            if (brush is SolidColorBrush)
            {
                return new SolidColorBrush(GetRandomRainbowMediaColor());
            }

            if (brush is LinearGradientBrush)
            {
                var randomBrush = (LinearGradientBrush) brush.CloneCurrentValue();
                var rand = GetRandomRainbowMediaColor();
                foreach (var stop in randomBrush.GradientStops)
                    stop.Color = System.Windows.Media.Color.FromArgb(stop.Color.A, rand.R, rand.G, rand.B);

                return randomBrush;
            }

            if (brush is RadialGradientBrush)
            {
                var randomBrush = (RadialGradientBrush) brush.CloneCurrentValue();
                var rand = GetRandomRainbowMediaColor();
                foreach (var stop in randomBrush.GradientStops)
                    stop.Color = System.Windows.Media.Color.FromArgb(stop.Color.A, rand.R, rand.G, rand.B);

                return randomBrush;
            }

            return brush;
        }
    }
}