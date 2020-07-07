using Artemis.Core.Plugins.Abstract.ViewModels;
using MaterialDesignThemes.Wpf;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor
{
    public class LayerBrushSettingsWindowViewModel : Conductor<BrushConfigurationViewModel>
    {
        public LayerBrushSettingsWindowViewModel(BrushConfigurationViewModel configurationViewModel)
        {
            ActiveItem = configurationViewModel;
        }
    }
}