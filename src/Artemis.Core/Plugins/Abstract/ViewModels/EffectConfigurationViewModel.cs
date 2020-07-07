using Artemis.Core.Plugins.LayerEffect.Abstract;
using Stylet;

namespace Artemis.Core.Plugins.Abstract.ViewModels
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