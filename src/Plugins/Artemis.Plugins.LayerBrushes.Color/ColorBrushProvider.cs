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

        protected override void EnablePlugin()
        {
        }

        protected override void DisablePlugin()
        {
        }
    }
}