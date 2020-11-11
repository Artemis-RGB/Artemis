using Artemis.Core.LayerEffects;
using Stylet;

namespace Artemis.UI.Shared.LayerEffects
{
    /// <summary>
    ///     Represents a view model for an effect configuration window
    /// </summary>
    public abstract class EffectConfigurationViewModel : Screen
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="EffectConfigurationViewModel" /> class
        /// </summary>
        /// <param name="layerEffect"></param>
        protected EffectConfigurationViewModel(BaseLayerEffect layerEffect)
        {
            LayerEffect = layerEffect;
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="EffectConfigurationViewModel" /> class with a validator
        /// </summary>
        /// <param name="layerEffect"></param>
        /// <param name="validator"></param>
        protected EffectConfigurationViewModel(BaseLayerEffect layerEffect, IModelValidator validator) : base(validator)
        {
            LayerEffect = layerEffect;
        }

        /// <summary>
        ///     Gets the layer effect this view model is associated with
        /// </summary>
        public BaseLayerEffect LayerEffect { get; }
    }
}