using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Routing;
using Artemis.UI.Shared.Routing;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using PropertyChanged.SourceGenerator;

namespace Artemis.UI.Screens.Workshop.Entries;

public partial class EntriesViewModel : RoutableHostScreen<RoutableScreen>
{
    private readonly IRouter _router;
    private ObservableAsPropertyHelper<bool>? _viewingDetails;
    [Notify] private RouteViewModel? _selectedTab;

    public EntriesViewModel(IRouter router)
    {
        _router = router;

        Tabs = new ObservableCollection<RouteViewModel>
        {
            new("Profiles", "workshop/entries/profiles/1", "workshop/entries/profiles"),
            new("Layouts", "workshop/entries/layouts/1", "workshop/entries/layouts")
        };

        this.WhenActivated(d =>
        {
            // Show back button on details page
            _viewingDetails = _router.CurrentPath.Select(p => p != null && p.Contains("details")).ToProperty(this, vm => vm.ViewingDetails).DisposeWith(d);
            // Navigate on tab change
            this.WhenAnyValue(vm => vm.SelectedTab)
                .WhereNotNull()
                .Subscribe(s => router.Navigate(s.Path, new RouterNavigationOptions {IgnoreOnPartialMatch = true, PartialMatchOverride = s.MatchPath}))
                .DisposeWith(d);
        });
    }

    public bool ViewingDetails => _viewingDetails?.Value ?? false;
    public ObservableCollection<RouteViewModel> Tabs { get; }

    public override async Task OnNavigating(NavigationArguments args, CancellationToken cancellationToken)
    {
        SelectedTab = Tabs.FirstOrDefault(t => t.Matches(args.Path));
        if (SelectedTab == null)
            await args.Router.Navigate(Tabs.First().Path);
    }

    public void GoBack()
    {
        if (ViewingDetails)
            _router.GoBack();
        else
            _router.Navigate("workshop");
    }
}