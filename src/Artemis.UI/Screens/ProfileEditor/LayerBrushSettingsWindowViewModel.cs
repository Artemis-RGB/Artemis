using Artemis.Core.Plugins.Abstract.ViewModels;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor
{
    public class LayerBrushSettingsWindowViewModel : Conductor<BrushConfigurationViewModel>
    {
        public LayerBrushSettingsWindowViewModel(BrushConfigurationViewModel configurationViewModel)
        {
            ActiveItem = configurationViewModel;
        }
    }
}