using Artemis.Core.LayerEffects;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor
{
    public class LayerEffectSettingsWindowViewModel : Conductor<EffectConfigurationViewModel>
    {
        public LayerEffectSettingsWindowViewModel(EffectConfigurationViewModel configurationViewModel)
        {
            ActiveItem = configurationViewModel;
        }
    }
}