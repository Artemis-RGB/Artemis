using Stylet;

namespace Artemis.Core.LayerEffects
{
    public abstract class EffectConfigurationViewModel : Screen
    {
        protected EffectConfigurationViewModel(BaseLayerEffect layerEffect)
        {
            LayerEffect = layerEffect;
        }

        public BaseLayerEffect LayerEffect { get; }
    }
}