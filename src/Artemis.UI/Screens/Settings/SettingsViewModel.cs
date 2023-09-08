using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Routing;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using ReactiveUI;

namespace Artemis.UI.Screens.Settings;

public class SettingsViewModel : RoutableHostScreen<RoutableScreen>, IMainScreenViewModel
{
    private readonly IRouter _router;
    private RouteViewModel? _selectedTab;

    public SettingsViewModel(IRouter router)
    {
        _router = router;
        SettingTabs = new ObservableCollection<RouteViewModel>
        {
            new("General", "settings/general"),
            new("Plugins", "settings/plugins"),
            new("Devices", "settings/devices"),
            new("Releases", "settings/releases"),
            new("About", "settings/about"),
        };
        
        // Navigate on tab change
        this.WhenActivated(d =>  this.WhenAnyValue(vm => vm.SelectedTab)
            .WhereNotNull()
            .Subscribe(s => _router.Navigate(s.Path, new RouterNavigationOptions {IgnoreOnPartialMatch = true}))
            .DisposeWith(d));
    }

    public ObservableCollection<RouteViewModel> SettingTabs { get; }

    public RouteViewModel? SelectedTab
    {
        get => _selectedTab;
        set => RaiseAndSetIfChanged(ref _selectedTab, value);
    }

    public ViewModelBase? TitleBarViewModel => null;
    
    /// <inheritdoc />
    public override async Task OnNavigating(NavigationArguments args, CancellationToken cancellationToken)
    {
        // Display tab change on navigate
        SelectedTab = SettingTabs.FirstOrDefault(t => t.Matches(args.Path));
        
        // Always show a tab, if there is none forward to the first
        if (SelectedTab == null)
            await _router.Navigate(SettingTabs.First().Path);
    }

    public void GoBack()
    {
        _router.Navigate("workshop");
    }
}