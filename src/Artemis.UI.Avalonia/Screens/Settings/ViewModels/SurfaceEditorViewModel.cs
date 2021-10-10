using ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Settings.ViewModels
{
    public class SettingsViewModel : MainScreenViewModel
    {
        public SettingsViewModel(IScreen hostScreens) : base(hostScreens, "settings")
        {
            DisplayName = "Settings";
        }
    }
}