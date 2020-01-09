using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.KeyframeEngines;
using Artemis.Core.Models.Profile.LayerProperties;
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

        /// <summary>
        ///     Instantiates and adds a compatible <see cref="KeyframeEngine" /> to the provided <see cref="LayerProperty{T}" />
        /// </summary>
        /// <param name="layerProperty">The layer property to apply the keyframe engine to.</param>
        /// <returns>The resulting keyframe engine, if a compatible engine was found.</returns>
        KeyframeEngine InstantiateKeyframeEngine<T>(LayerProperty<T> layerProperty);

        /// <summary>
        ///     Instantiates and adds a compatible <see cref="KeyframeEngine" /> to the provided <see cref="BaseLayerProperty" />.
        /// </summary>
        /// <param name="layerProperty">The layer property to apply the keyframe engine to.</param>
        /// <returns>The resulting keyframe engine, if a compatible engine was found.</returns>
        KeyframeEngine InstantiateKeyframeEngine(BaseLayerProperty layerProperty);
    }
}