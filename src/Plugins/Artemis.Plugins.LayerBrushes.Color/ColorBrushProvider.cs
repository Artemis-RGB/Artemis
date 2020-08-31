using Artemis.Core.LayerBrushes;

namespace Artemis.Plugins.LayerBrushes.Color
{
    public class ColorBrushProvider : LayerBrushProvider
    {
        public override void EnablePlugin()
        {
            RegisterLayerBrushDescriptor<ColorBrush>("Color", "A brush supporting solid colors and multiple types of gradients", "Brush");
        }

        public override void DisablePlugin()
        {
        }
    }
}