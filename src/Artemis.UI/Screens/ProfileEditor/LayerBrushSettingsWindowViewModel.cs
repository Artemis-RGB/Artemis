using Artemis.Core.LayerBrushes;
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