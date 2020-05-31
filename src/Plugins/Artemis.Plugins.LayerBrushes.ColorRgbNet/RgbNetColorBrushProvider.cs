using Artemis.Core.Plugins.LayerBrush;
using Artemis.Core.Plugins.Models;

namespace Artemis.Plugins.LayerBrushes.ColorRgbNet
{
    public class RgbNetColorBrushProvider : LayerBrushProvider
    {
        public RgbNetColorBrushProvider(PluginInfo pluginInfo) : base(pluginInfo)
        {
            AddLayerBrushDescriptor<RgbNetColorBrush>("RGB.NET Color", "A RGB.NET based color", "Brush");
        }

        public override void EnablePlugin()
        {
        }

        public override void DisablePlugin()
        {
        }

        public override void Dispose()
        {
        }
    }
}