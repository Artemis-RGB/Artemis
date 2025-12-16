using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables.Fluent;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Routing;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Settings;

public partial class SettingsViewModel : RoutableHostScreen<RoutableScreen>, IMainScreenViewModel
{
    private readonly IRouter _router;
    [Notify] private RouteViewModel? _selectedTab;

    public SettingsViewModel(IRouter router)
    {
        _router = router;
        SettingTabs = new ObservableCollection<RouteViewModel>
        {
            new("General", "settings/general"),
            new("Plugins", "settings/plugins"),
            new("Devices", "settings/devices"),
            new("Releases", "settings/releases"),
            new("Account", "settings/account"),
            new("About", "settings/about"),
        };
        
        // Navigate on tab change
        this.WhenActivated(d =>  this.WhenAnyValue(vm => vm.SelectedTab)
            .WhereNotNull()
            .Subscribe(s => _router.Navigate(s.Path, new RouterNavigationOptions {IgnoreOnPartialMatch = true}))
            .DisposeWith(d));
    }

    public ObservableCollection<RouteViewModel> SettingTabs { get; }

    public ViewModelBase? TitleBarViewModel => null;
    
    /// <inheritdoc />
    public override Task OnNavigating(NavigationArguments args, CancellationToken cancellationToken)
    {
        // Display tab change on navigate, if there is none forward to the first
        SelectedTab = SettingTabs.FirstOrDefault(t => t.Matches(args.Path)) ?? SettingTabs.FirstOrDefault();
        return Task.CompletedTask;
    }

    public void GoBack()
    {
        _router.Navigate("workshop");
    }
}