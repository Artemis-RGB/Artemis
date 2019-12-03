using System.Collections.Generic;
using Artemis.Core.Plugins.LayerElement;
using SkiaSharp;

namespace Artemis.Plugins.LayerElements.Brush
{
    public class BrushLayerElementSettings : LayerElementSettings
    {
        public BrushLayerElementSettings()
        {
            Colors = new List<SKColor>();
        }

        public List<SKColor> Colors { get; set; }
    }
}