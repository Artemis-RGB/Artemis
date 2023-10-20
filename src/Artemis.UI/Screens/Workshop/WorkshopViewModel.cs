using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.Home;
using Artemis.UI.Screens.Workshop.Search;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using PropertyChanged.SourceGenerator;

namespace Artemis.UI.Screens.Workshop;

public partial class WorkshopViewModel : RoutableHostScreen<RoutableScreen>, IMainScreenViewModel
{
    private readonly SearchViewModel _searchViewModel;
    [Notify] private ViewModelBase? _titleBarViewModel;

    public WorkshopViewModel(SearchViewModel searchViewModel, WorkshopHomeViewModel homeViewModel)
    {
        _searchViewModel = searchViewModel;
        HomeViewModel = homeViewModel;
    }

    public WorkshopHomeViewModel HomeViewModel { get; }

    /// <inheritdoc />
    public override Task OnNavigating(NavigationArguments args, CancellationToken cancellationToken)
    {
        TitleBarViewModel = args.Path == "workshop" ? _searchViewModel : null;
        return base.OnNavigating(args, cancellationToken);
    }
}