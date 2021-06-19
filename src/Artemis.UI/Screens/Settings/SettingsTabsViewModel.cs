using Artemis.UI.Screens.Settings.Tabs.About;
using Artemis.UI.Screens.Settings.Tabs.Devices;
using Artemis.UI.Screens.Settings.Tabs.General;
using Artemis.UI.Screens.Settings.Tabs.Plugins;
using Stylet;

namespace Artemis.UI.Screens.Settings
{
    public class SettingsTabsViewModel : Conductor<Screen>.Collection.OneActive
    {
        public SettingsTabsViewModel(
            GeneralSettingsTabViewModel generalSettingsTabViewModel,
            PluginSettingsTabViewModel pluginSettingsTabViewModel,
            DeviceSettingsTabViewModel deviceSettingsTabViewModel,
            AboutTabViewModel aboutTabViewModel)
        {
            DisplayName = "Settings";

            Items.Add(generalSettingsTabViewModel);
            Items.Add(pluginSettingsTabViewModel);
            Items.Add(deviceSettingsTabViewModel);
            Items.Add(aboutTabViewModel);

            ActiveItem = generalSettingsTabViewModel;
        }
    }
}