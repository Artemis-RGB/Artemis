using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.LayerBrush;
using Artemis.Core.Plugins.Models;

namespace Artemis.Plugins.LayerBrushes.Color
{
    public class ColorBrushProvider : LayerBrushProvider
    {
        public ColorBrushProvider(PluginInfo pluginInfo) : base(pluginInfo)
        {
            AddLayerBrushDescriptor<ColorBrush>("Color", "A color with an (optional) gradient", "Brush");
        }

        public override void EnablePlugin()
        {
        }

        public override void DisablePlugin()
        {
        }
    }
}