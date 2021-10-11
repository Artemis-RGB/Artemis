using ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Settings.ViewModels
{
    public class SettingsViewModel : MainScreenViewModel
    {
        public SettingsViewModel(IScreen hostScreen) : base(hostScreen, "settings")
        {
            DisplayName = "Settings";
        }
    }
}