using Artemis.Core.Plugins.LayerElement;

namespace Artemis.Plugins.LayerElements.Noise
{
    public class NoiseLayerElementViewModel : LayerElementViewModel
    {
        public NoiseLayerElementViewModel(NoiseLayerElement layerElement) : base(layerElement)
        {
            LayerElement = layerElement;
        }

        public new NoiseLayerElement LayerElement { get; }
    }
}