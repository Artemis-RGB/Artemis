using Artemis.Core.Plugins.LayerElement;

namespace Artemis.Plugins.LayerElements.Brush
{
    public class BrushLayerElementViewModel : LayerElementViewModel
    {
        public BrushLayerElementViewModel(BrushLayerElement layerElement) : base(layerElement)
        {
            LayerElement = layerElement;
        }

        public new BrushLayerElement LayerElement { get; }
    }
}