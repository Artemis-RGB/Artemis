using Stylet;

namespace Artemis.Core.LayerEffects
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
        ///     Gets the layer effect this view model is associated with
        /// </summary>
        public BaseLayerEffect LayerEffect { get; }
    }
}