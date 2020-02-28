using System.Collections.Generic;
using SkiaSharp;

namespace Artemis.Core.Models.Profile
{
    public class ColorGradient
    {
        public ColorGradient()
        {
            Colors = new List<ColorGradientColor>();
        }

        public List<ColorGradientColor> Colors { get; }
        public float Rotation { get; set; }
    }

    public struct ColorGradientColor
    {
        public ColorGradientColor(SKColor color, float position)
        {
            Color = color;
            Position = position;
        }

        public SKColor Color { get; set; }
        public float Position { get; set; }
    }
}