using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Artemis.Utilities
{
    public static class ColorHelpers
    {
        /// <summary>
        ///     Comes up with a 'pure' psuedo-random color
        /// </summary>
        /// <returns>The color</returns>
        public static Color GetRandomRainbowColor()
        {
            var colors = new List<int>();
            var rand = new Random();
            for (var i = 0; i < 3; i++)
                colors.Add(rand.Next(0, 256));

            var highest = colors.Max();
            var lowest = colors.Min();
            colors[colors.FindIndex(c => c == highest)] = 255;
            colors[colors.FindIndex(c => c == lowest)] = 0;

            var returnColor = Color.FromArgb(255, colors[0], colors[1], colors[2]);

            return returnColor;
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
    }
}