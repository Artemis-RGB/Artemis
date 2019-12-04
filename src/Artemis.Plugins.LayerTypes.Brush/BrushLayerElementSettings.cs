using System.Collections.Generic;
using System.ComponentModel;
using Artemis.Core.Plugins.LayerElement;
using SkiaSharp;

namespace Artemis.Plugins.LayerElements.Brush
{
    public class BrushLayerElementSettings : LayerElementSettings
    {
        private BrushType _brushType;
        private List<SKColor> _colors;

        public BrushLayerElementSettings()
        {
            BrushType = BrushType.Solid;
            Colors = new List<SKColor>();
        }

        public BrushType BrushType
        {
            get => _brushType;
            set => SetAndNotify(ref _brushType, value);
        }

        public List<SKColor> Colors
        {
            get => _colors;
            set => SetAndNotify(ref _colors, value);
        }
    }

    public enum BrushType
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