using Artemis.Core.Plugins.LayerElement;
using Artemis.Core.Plugins.Models;

namespace Artemis.Plugins.LayerElements.Noise
{
    public class NoiseLayerElementProvider : LayerElementProvider
    {
        public NoiseLayerElementProvider(PluginInfo pluginInfo) : base(pluginInfo)
        {
            AddLayerElementDescriptor<NoiseLayerElement>("Noise", "A brush of that shows an animated random noise", "ScatterPlot");
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