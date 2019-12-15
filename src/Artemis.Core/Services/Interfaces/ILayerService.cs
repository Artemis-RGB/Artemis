using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.LayerBrush;

namespace Artemis.Core.Services.Interfaces
{
    public interface ILayerService : IArtemisService
    {
        /// <summary>
        ///     Instantiates and adds the <see cref="LayerBrush" /> described by the provided
        ///     <see cref="LayerBrushDescriptor" /> to the provided <see cref="Layer" />.
        /// </summary>
        /// <param name="layer">The layer to add the new layer element to</param>
        /// <param name="brushDescriptor">The descriptor of the new layer brush</param>
        /// <param name="settings">JSON settings to be deserialized and injected into the layer brush</param>
        /// <returns></returns>
        LayerBrush InstantiateLayerBrush(Layer layer, LayerBrushDescriptor brushDescriptor, string settings = null);
    }
}