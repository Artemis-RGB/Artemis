using System.Collections.ObjectModel;
using Artemis.UI.Shared;
using ReactiveUI;

namespace Artemis.UI.Screens.Settings;

public class SettingsViewModel : MainScreenViewModel
{
    private ActivatableViewModelBase _selectedTab;

    public SettingsViewModel(IScreen hostScreen,
        GeneralTabViewModel generalTabViewModel,
        PluginsTabViewModel pluginsTabViewModel,
        DevicesTabViewModel devicesTabViewModel,
        ReleasesTabViewModel releasesTabViewModel,
        AboutTabViewModel aboutTabViewModel) : base(hostScreen, "settings")
    {
        SettingTabs = new ObservableCollection<ActivatableViewModelBase>
        {
            generalTabViewModel,
            pluginsTabViewModel,
            devicesTabViewModel,
            releasesTabViewModel,
            aboutTabViewModel
        };
    }

    public ObservableCollection<ActivatableViewModelBase> SettingTabs { get; }

    public ActivatableViewModelBase SelectedTab
    {
        get => _selectedTab;
        set => RaiseAndSetIfChanged(ref _selectedTab, value);
    }
}