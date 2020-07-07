using Artemis.Core.Plugins.Abstract.ViewModels;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor
{
    public class LayerEffectSettingsWindowViewModel : Conductor<EffectConfigurationViewModel>
    {
        public LayerEffectSettingsWindowViewModel(EffectConfigurationViewModel configurationViewModel)
        {
            ActiveItem = configurationViewModel;
        }
    }
}