using Artemis.Core.Plugins.LayerBrushes;

namespace Artemis.Plugins.LayerBrushes.Noise
{
    public class NoiseBrushProvider : LayerBrushProvider
    {
        public override void EnablePlugin()
        {
            RegisterLayerBrushDescriptor<NoiseBrush>("Noise", "A brush of that shows an animated random noise", "ScatterPlot");
        }

        public override void DisablePlugin()
        {
        }
    }
}