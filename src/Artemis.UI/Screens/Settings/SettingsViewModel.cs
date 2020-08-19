using Artemis.UI.Screens.Settings.Tabs.Devices;
using Artemis.UI.Screens.Settings.Tabs.General;
using Artemis.UI.Screens.Settings.Tabs.Modules;
using Artemis.UI.Screens.Settings.Tabs.Plugins;
using Stylet;

namespace Artemis.UI.Screens.Settings
{
    public class SettingsViewModel : Conductor<Screen>.Collection.OneActive, IMainScreenViewModel
    {
        public SettingsViewModel(
            GeneralSettingsTabViewModel generalSettingsTabViewModel,
            ModuleOrderTabViewModel moduleOrderTabViewModel,
            PluginSettingsTabViewModel pluginSettingsTabViewModel,
            DeviceSettingsTabViewModel deviceSettingsTabViewModel)
        {
            DisplayName = "Settings";

            Items.Add(generalSettingsTabViewModel);
            Items.Add(moduleOrderTabViewModel);
            Items.Add(pluginSettingsTabViewModel);
            Items.Add(deviceSettingsTabViewModel);

            ActiveItem = generalSettingsTabViewModel;
        }
    }
}