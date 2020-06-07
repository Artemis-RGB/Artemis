using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.LayerBrush;
using Artemis.Core.Plugins.Models;

namespace Artemis.Plugins.LayerBrushes.Noise
{
    public class NoiseBrushProvider : LayerBrushProvider
    {
        public NoiseBrushProvider(PluginInfo pluginInfo) : base(pluginInfo)
        {
            AddLayerBrushDescriptor<NoiseBrush>("Noise", "A brush of that shows an animated random noise", "ScatterPlot");
        }

        public override void EnablePlugin()
        {
        }

        public override void DisablePlugin()
        {
        }
    }
}