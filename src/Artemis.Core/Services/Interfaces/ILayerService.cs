using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.LayerElement;

namespace Artemis.Core.Services.Interfaces
{
    public interface ILayerService : IArtemisService
    {
        /// <summary>
        ///     Instantiates and adds the <see cref="LayerElement" /> described by the provided
        ///     <see cref="LayerElementDescriptor" /> to the provided <see cref="Layer" />.
        /// </summary>
        /// <param name="layer">The layer to add the new layer element to</param>
        /// <param name="layerElementDescriptor">The descriptor of the new layer element</param>
        /// <param name="settings">JSON settings to be deserialized and injected into the layer element</param>
        /// <returns></returns>
        LayerElement InstantiateLayerElement(Layer layer, LayerElementDescriptor layerElementDescriptor, string settings = null);
    }
}