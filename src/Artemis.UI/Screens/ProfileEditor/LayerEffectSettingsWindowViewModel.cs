using Artemis.Core.Plugins.LayerEffects;
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