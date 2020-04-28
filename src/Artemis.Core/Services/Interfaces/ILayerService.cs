using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.KeyframeEngines;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Plugins.LayerBrush;

namespace Artemis.Core.Services.Interfaces
{
    public interface ILayerService : IArtemisService
    {
        /// <summary>
        ///     Instantiates and adds the <see cref="LayerBrush" /> described by the provided <see cref="LayerBrushDescriptor" />
        ///     to the <see cref="Layer" />.
        /// </summary>
        /// <param name="layer">The layer to instantiate the brush for</param>
        /// <returns></returns>
        ILayerBrush InstantiateLayerBrush(Layer layer);

        void LoadPropertyBaseValue(Layer layer, string path, object layerProperty);
        void LoadPropertyKeyframes(Layer layer, string path, object layerProperty);
    }
}