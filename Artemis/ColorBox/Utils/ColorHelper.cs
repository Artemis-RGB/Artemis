/*****************   NCore Softwares Pvt. Ltd., India   **************************

   ColorBox

   Copyright (C) 2013 NCore Softwares Pvt. Ltd.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://colorbox.codeplex.com/license

***********************************************************************************/

using System;
using System.Globalization;
using System.Windows.Media;

namespace ColorBox
{
    internal static class ColorHelper
    {
        private static readonly char[] HexArray =
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D',
            'E', 'F'
        };

        public static string MakeValidColorString(string S)
        {
            var s = S;

            for (var i = 0; i < s.Length; i++)
            {
                var c = s[i];

                if (!((c >= 'a') && (c <= 'f')) && !((c >= 'A') && (c <= 'F')) && !((c >= '0') && (c <= '9')))
                {
                    s = s.Remove(i, 1);
                    i--;
                }
            }

            if (s.Length > 8) s = s.Substring(0, 8);

            while ((s.Length <= 8) && (s.Length != 3) && (s.Length != 4) && (s.Length != 6) && (s.Length != 8))
                s = s + "0";

            return s;
        }

        public static Color ColorFromString(string S)
        {
            var s = MakeValidColorString(S);

            byte a = 255;
            byte r = 0;
            byte g = 0;
            byte b = 0;

            if (s.Length == 3)
            {
                r = byte.Parse(s.Substring(0, 1) + s.Substring(0, 1), NumberStyles.HexNumber);
                g = byte.Parse(s.Substring(1, 1) + s.Substring(1, 1), NumberStyles.HexNumber);
                b = byte.Parse(s.Substring(2, 1) + s.Substring(2, 1), NumberStyles.HexNumber);
            }

            if (s.Length == 4)
            {
                a = byte.Parse(s.Substring(0, 1) + s.Substring(0, 1), NumberStyles.HexNumber);
                r = byte.Parse(s.Substring(1, 1) + s.Substring(1, 1), NumberStyles.HexNumber);
                g = byte.Parse(s.Substring(2, 1) + s.Substring(2, 1), NumberStyles.HexNumber);
                b = byte.Parse(s.Substring(3, 1) + s.Substring(3, 1), NumberStyles.HexNumber);
            }

            if (s.Length == 6)
            {
                r = byte.Parse(s.Substring(0, 2), NumberStyles.HexNumber);
                g = byte.Parse(s.Substring(1, 2), NumberStyles.HexNumber);
                b = byte.Parse(s.Substring(2, 2), NumberStyles.HexNumber);
            }

            if (s.Length == 8)
            {
                a = byte.Parse(s.Substring(0, 2), NumberStyles.HexNumber);
                r = byte.Parse(s.Substring(1, 2), NumberStyles.HexNumber);
                g = byte.Parse(s.Substring(2, 2), NumberStyles.HexNumber);
                b = byte.Parse(s.Substring(3, 2), NumberStyles.HexNumber);
            }

            return Color.FromArgb(a, r, g, b);
        }

        public static string StringFromColor(Color c)
        {
            var bytes = new byte[4] {c.A, c.R, c.G, c.B};

            var chars = new char[bytes.Length*2];

            for (var i = 0; i < bytes.Length; i++)
            {
                int b = bytes[i];
                chars[i*2] = HexArray[b >> 4];
                chars[i*2 + 1] = HexArray[b & 0xF];
            }

            return new string(chars);
        }

        public static Color ColorFromHsb(double H, double S, double b)
        {
            double red = 0.0, green = 0.0, blue = 0.0;

            if (S == 0.0)
            {
                red = green = blue = b;
            }
            else
            {
                var h = H*360;
                while (h >= 360.0)
                    h -= 360.0;

                h = h/60.0;
                var i = (int) h;

                var f = h - i;
                var r = b*(1.0 - S);
                var s = b*(1.0 - S*f);
                var t = b*(1.0 - S*(1.0 - f));

                switch (i)
                {
                    case 0:
                        red = b;
                        green = t;
                        blue = r;
                        break;
                    case 1:
                        red = s;
                        green = b;
                        blue = r;
                        break;
                    case 2:
                        red = r;
                        green = b;
                        blue = t;
                        break;
                    case 3:
                        red = r;
                        green = s;
                        blue = b;
                        break;
                    case 4:
                        red = t;
                        green = r;
                        blue = b;
                        break;
                    case 5:
                        red = b;
                        green = r;
                        blue = s;
                        break;
                }
            }

            byte iRed = (byte) (red*255.0), iGreen = (byte) (green*255.0), iBlue = (byte) (blue*255.0);
            return Color.FromRgb(iRed, iGreen, iBlue);
        }

        public static void HsbFromColor(Color c, ref double h, ref double s, ref double b)
        {
            var red = c.R;
            var green = c.G;
            var blue = c.B;

            int imax = red, imin = red;

            if (green > imax) imax = green;
            else if (green < imin) imin = green;
            if (blue > imax) imax = blue;
            else if (blue < imin) imin = blue;
            double max = imax/255.0, min = imin/255.0;

            var value = max;
            var saturation = max > 0 ? (max - min)/max : 0.0;
            double hue = 0;

            if (imax > imin)
            {
                var f = 1.0/((max - min)*255.0);
                hue = imax == red
                    ? 0.0 + f*(green - blue)
                    : imax == green ? 2.0 + f*(blue - red) : 4.0 + f*(red - green);
                hue = hue*60.0;
                if (hue < 0.0)
                    hue += 360.0;
            }

            h = hue/360;
            s = saturation;
            b = value;
        }

        public static Color ColorFromAhsb(double a, double h, double s, double b)
        {
            var r = ColorFromHsb(h, s, b);
            r.A = (byte) Math.Round(a*255);
            return r;
        }
    }
}