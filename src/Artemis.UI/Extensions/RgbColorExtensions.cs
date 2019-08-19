using System;
using RGB.NET.Core;
using Color = System.Windows.Media.Color;

namespace Artemis.UI.Extensions
{
    public static class RgbColorExtensions
    {
        public static Color ToMediaColor(this RGB.NET.Core.Color color)
        {
            var (_, r, g, b) = color.GetRGBBytes();
            return Color.FromRgb(r, g, b);
        }
    }
}