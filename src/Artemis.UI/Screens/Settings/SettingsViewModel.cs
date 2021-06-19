using Stylet;

namespace Artemis.UI.Screens.Settings
{
    public class SettingsViewModel : MainScreenViewModel
    {
        public SettingsViewModel(SettingsTabsViewModel settingsTabsViewModel)
        {
            DisplayName = "Settings";

            settingsTabsViewModel.ConductWith(this);
            ActiveItem = settingsTabsViewModel;
        }

        public SettingsTabsViewModel ActiveItem { get; }
    }
}