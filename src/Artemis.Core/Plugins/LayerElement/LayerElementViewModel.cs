using Stylet;

namespace Artemis.Core.Plugins.LayerElement
{
    public abstract class LayerElementViewModel : PropertyChangedBase
    {
        protected LayerElementViewModel(LayerElement layerElement)
        {
            LayerElement = layerElement;
        }

        public LayerElement LayerElement { get; }
    }
}