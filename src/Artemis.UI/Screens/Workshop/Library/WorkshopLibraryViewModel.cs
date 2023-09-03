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

namespace Artemis.UI.Screens.Workshop.Library;

public class WorkshopLibraryViewModel : RoutableHostScreen<RoutableScreen>
{
    private readonly IRouter _router;
    private RouteViewModel? _selectedTab;
    private ObservableAsPropertyHelper<bool>? _viewingDetails;

    /// <inheritdoc />
    public WorkshopLibraryViewModel(IRouter router)
    {
        _router = router;

        Tabs = new ObservableCollection<RouteViewModel>
        {
            new("Installed", "workshop/library/installed"),
            new("Submissions", "workshop/library/submissions")
        };

        this.WhenActivated(d =>
        {
            _viewingDetails = _router.CurrentPath.Select(p => p != null && p.StartsWith("workshop/library/submissions/")).ToProperty(this, vm => vm.ViewingDetails).DisposeWith(d);
            // Navigate on tab change
            this.WhenAnyValue(vm => vm.SelectedTab)
                .WhereNotNull()
                .Subscribe(s => router.Navigate(s.Path, new RouterNavigationOptions {IgnoreOnPartialMatch = true}))
                .DisposeWith(d);
        });
    }

    public bool ViewingDetails => _viewingDetails?.Value ?? false;
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

    public void GoBack()
    {
        if (ViewingDetails)
            _router.GoBack();
        else
            _router.Navigate("workshop");
    }
}