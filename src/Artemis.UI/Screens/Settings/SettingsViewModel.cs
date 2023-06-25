using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Avalonia.Threading;
using ReactiveUI;


namespace Artemis.UI.Screens.Settings;

public class SettingsViewModel : ActivatableRoutable, IMainScreenViewModel
{
    private readonly IRouter _router;
    private SettingsTab? _selectedTab;

    public SettingsViewModel(IRouter router)
    {
        _router = router;
        SettingTabs = new ObservableCollection<SettingsTab>
        {
            new("general", "General"),
            new("plugins", "Plugins"),
            new("devices", "Devices"),
            new("releases", "Releases"),
            new("about", "About"),
        };
        _selectedTab = SettingTabs.First();


        this.WhenActivated(d =>
        {
            _router.CurrentPath.WhereNotNull().Subscribe(p =>
            {
                if (!p.StartsWith("settings"))
                    return;
                
                SettingsTab tab = SettingTabs.FirstOrDefault(t => t.Matches(p)) ?? SettingTabs.First();
                if (SelectedTab != tab)
                    SelectedTab = tab;
            }).DisposeWith(d);
        });
    }

    public ObservableCollection<SettingsTab> SettingTabs { get; }

    public SettingsTab? SelectedTab
    {
        get => _selectedTab;
        set
        {
            RaiseAndSetIfChanged(ref _selectedTab, value);
            NavigateToTab(_selectedTab);
        }
    }

    private void NavigateToTab(SettingsTab? selectedTab)
    {
        if (selectedTab != null)
            Dispatcher.UIThread.InvokeAsync(async () => await _router.Navigate("settings/" + selectedTab.Path));
    }

    public ViewModelBase? TitleBarViewModel => null;
}