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
        LayerBrush InstantiateLayerBrush(Layer layer);

        /// <summary>
        ///     Instantiates and adds a compatible <see cref="KeyframeEngine" /> to the provided <see cref="LayerProperty{T}" />.
        ///     If the property already has a compatible keyframe engine, nothing happens.
        /// </summary>
        /// <param name="layerProperty">The layer property to apply the keyframe engine to.</param>
        /// <returns>The resulting keyframe engine, if a compatible engine was found.</returns>
        KeyframeEngine InstantiateKeyframeEngine<T>(LayerProperty<T> layerProperty);

        /// <summary>
        ///     Instantiates and adds a compatible <see cref="KeyframeEngine" /> to the provided <see cref="BaseLayerProperty" />.
        ///     If the property already has a compatible keyframe engine, nothing happens.
        /// </summary>
        /// <param name="layerProperty">The layer property to apply the keyframe engine to.</param>
        /// <returns>The resulting keyframe engine, if a compatible engine was found.</returns>
        KeyframeEngine InstantiateKeyframeEngine(BaseLayerProperty layerProperty);
    }
}