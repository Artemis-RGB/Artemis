using System;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.Home;
using Artemis.UI.Screens.Workshop.Search;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;

namespace Artemis.UI.Screens.Workshop;

public class WorkshopViewModel : RoutableScreen<IWorkshopViewModel>, IMainScreenViewModel
{
    private readonly SearchViewModel _searchViewModel;

    public WorkshopViewModel(SearchViewModel searchViewModel, WorkshopHomeViewModel homeViewModel)
    {
        _searchViewModel = searchViewModel;
        
        TitleBarViewModel = searchViewModel;
        HomeViewModel = homeViewModel;
    }

    public ViewModelBase TitleBarViewModel { get; }
    public WorkshopHomeViewModel HomeViewModel { get; }

    /// <inheritdoc />
    public override Task OnNavigating(NavigationArguments args, CancellationToken cancellationToken)
    {
        _searchViewModel.EntryType = Screen?.EntryType;
        return Task.CompletedTask;
    }
}

public interface IWorkshopViewModel
{
    public EntryType? EntryType { get; }
}