using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Routing;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;
using ReactiveUI;
using System;

namespace Artemis.UI.Screens.Workshop.Library;

public class WorkshopLibraryViewModel : RoutableHostScreen<RoutableScreen>
{
    private RouteViewModel? _selectedTab;

    /// <inheritdoc />
    public WorkshopLibraryViewModel(IRouter router)
    {
        Tabs = new ObservableCollection<RouteViewModel>
        {
            new("workshop/library/installed", "Installed"),
            new("workshop/library/submissions", "Submissions")
        };
        
        // Navigate on tab change
        this.WhenActivated(d =>  this.WhenAnyValue(vm => vm.SelectedTab)
            .WhereNotNull()
            .Subscribe(s => router.Navigate(s.Path, new RouterNavigationOptions {IgnoreOnPartialMatch = true}))
            .DisposeWith(d));
    }

    public EntryType? EntryType => null;
    public ObservableCollection<RouteViewModel> Tabs { get; }

    public RouteViewModel? SelectedTab
    {
        get => _selectedTab;
        set => RaiseAndSetIfChanged(ref _selectedTab, value);
    }

    public override async Task OnNavigating(NavigationArguments args, CancellationToken cancellationToken)
    {
        SelectedTab = Tabs.FirstOrDefault(t => t.Matches(args.Path));
        if (SelectedTab == null)
            await args.Router.Navigate(Tabs.First().Path);
    }
}