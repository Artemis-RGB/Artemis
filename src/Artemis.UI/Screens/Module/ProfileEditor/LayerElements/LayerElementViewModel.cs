using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.LayerElement;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerElements
{
    public class LayerElementViewModel
    {
        public LayerElementViewModel(LayerElement layerElement)
        {
            Layer = layerElement.Layer;
            LayerElement = layerElement;
            LayerElementDescriptor = layerElement.Descriptor;
        }

        public Layer Layer { get; set; }
        public LayerElement LayerElement { get; set; }
        public LayerElementDescriptor LayerElementDescriptor { get; set; }
    }
}