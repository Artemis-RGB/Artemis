using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using ReactiveUI;

namespace Artemis.UI.Screens.Settings;

public class SettingsViewModel : RoutableScreen<ActivatableViewModelBase>, IMainScreenViewModel
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
        
        // Navigate on tab change
        this.WhenActivated(d =>  this.WhenAnyValue(vm => vm.SelectedTab)
            .WhereNotNull()
            .Subscribe(s => _router.Navigate($"settings/{s.Path}", new RouterNavigationOptions {IgnoreOnPartialMatch = true}))
            .DisposeWith(d));
    }

    public ObservableCollection<SettingsTab> SettingTabs { get; }

    public SettingsTab? SelectedTab
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
            await _router.Navigate($"settings/{SettingTabs.First().Path}");
    }
}