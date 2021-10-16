using System.Collections.ObjectModel;
using ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Settings.ViewModels
{
    public class SettingsViewModel : MainScreenViewModel
    {
        public SettingsViewModel(IScreen hostScreen,
            GeneralTabViewModel generalTabViewModel,
            PluginsTabViewModel pluginsTabViewModel,
            DevicesTabViewModel devicesTabViewModel,
            AboutTabViewModel aboutTabViewModel) : base(hostScreen, "settings")
        {
            SettingTabs = new ObservableCollection<ActivatableViewModelBase>
            {
                generalTabViewModel,
                pluginsTabViewModel,
                devicesTabViewModel,
                aboutTabViewModel
            };
        }

        public ObservableCollection<ActivatableViewModelBase> SettingTabs { get; }
    }
}