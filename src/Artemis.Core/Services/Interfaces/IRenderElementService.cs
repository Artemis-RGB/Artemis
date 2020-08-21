using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.LayerBrushes;
using Artemis.Core.Plugins.LayerBrushes.Internal;
using Artemis.Core.Plugins.LayerEffects;

namespace Artemis.Core.Services.Interfaces
{
    public interface IRenderElementService : IArtemisService
    {
        /// <summary>
        ///     Creates a new layer
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        Layer CreateLayer(Profile profile, ProfileElement parent, string name);

        /// <summary>
        ///     Removes the currently active layer brush from the <see cref="Layer" /> and deletes any settings     
        /// </summary>
        /// <param name="layer">The layer to remove the active brush from</param>
        void RemoveLayerBrush(Layer layer);

        /// <summary>
        ///     Deactivates the currently active layer brush from the <see cref="Layer" /> but keeps all settings     
        /// </summary>
        /// <param name="layer">The layer to deactivate the active brush on</param>
        void DeactivateLayerBrush(Layer layer);

        /// <summary>
        ///     Instantiates and adds the <see cref="BaseLayerBrush" /> described by the provided
        ///     <see cref="LayerBrushDescriptor" />
        ///     to the <see cref="Layer" />.
        /// </summary>
        /// <param name="layer">The layer to instantiate the brush for</param>
        /// <returns></returns>
        BaseLayerBrush InstantiateLayerBrush(Layer layer);

        /// <summary>
        ///     Instantiates and adds the <see cref="BaseLayerEffect" /> described by the provided
        ///     <see cref="LayerEffectDescriptor" /> to the <see cref="Layer" />.
        /// </summary>
        /// <param name="renderProfileElement">The layer/folder to instantiate the effect for</param>
        void InstantiateLayerEffects(RenderProfileElement renderProfileElement);

        /// <summary>
        ///     Adds the <see cref="BaseLayerEffect" /> described by the provided <see cref="LayerEffectDescriptor" /> to the
        ///     <see cref="Layer" />.
        /// </summary>
        /// <param name="renderProfileElement">The layer/folder to instantiate the effect for</param>
        /// <param name="layerEffectDescriptor"></param>
        /// <returns></returns>
        BaseLayerEffect AddLayerEffect(RenderProfileElement renderProfileElement, LayerEffectDescriptor layerEffectDescriptor);

        void RemoveLayerEffect(BaseLayerEffect layerEffect);

        void InstantiateDisplayConditions(RenderProfileElement renderElement);
    }
}