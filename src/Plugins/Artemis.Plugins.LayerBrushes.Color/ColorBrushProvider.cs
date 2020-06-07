using Artemis.Core.Plugins.Abstract;

namespace Artemis.Plugins.LayerBrushes.Color
{
    public class ColorBrushProvider : LayerBrushProvider
    {
        public override void EnablePlugin()
        {
            AddLayerBrushDescriptor<ColorBrush>("Color", "A color with an (optional) gradient", "Brush");
        }

        public override void DisablePlugin()
        {
        }
    }
}