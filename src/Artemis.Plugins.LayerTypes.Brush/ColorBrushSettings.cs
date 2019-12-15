using System.Collections.Generic;
using System.ComponentModel;
using Artemis.Core.Plugins.LayerBrush;
using SkiaSharp;

namespace Artemis.Plugins.LayerBrushes.Color
{
    public class ColorBrushSettings : LayerBrushSettings
    {
        private List<SKColor> _colors;
        private GradientType _gradientType;

        public ColorBrushSettings()
        {
            GradientType = GradientType.Solid;
            Colors = new List<SKColor>();
        }

        public GradientType GradientType
        {
            get => _gradientType;
            set => SetAndNotify(ref _gradientType, value);
        }

        public List<SKColor> Colors
        {
            get => _colors;
            set => SetAndNotify(ref _colors, value);
        }
    }

    public enum GradientType
    {
        [Description("Solid")]
        Solid,

        [Description("Linear Gradient")]
        LinearGradient,

        [Description("Radial Gradient")]
        RadialGradient,

        [Description("Sweep Gradient")]
        SweepGradient
    }
}