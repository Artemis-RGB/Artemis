using Artemis.Core.Plugins.LayerElement;
using Artemis.Core.Plugins.Models;

namespace Artemis.Plugins.LayerElements.Brush
{
    public class BrushLayerElementProvider : LayerElementProvider
    {
        public BrushLayerElementProvider(PluginInfo pluginInfo) : base(pluginInfo)
        {
            AddLayerElementDescriptor<BrushLayerElement>("Brush", "A brush of a specific type and colors", "Brush");
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