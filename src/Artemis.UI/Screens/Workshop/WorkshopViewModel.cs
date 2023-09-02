using Artemis.UI.Screens.Workshop.Home;
using Artemis.UI.Screens.Workshop.Search;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;

namespace Artemis.UI.Screens.Workshop;

public class WorkshopViewModel : RoutableHostScreen<RoutableScreen>, IMainScreenViewModel
{
    public WorkshopViewModel(SearchViewModel searchViewModel, WorkshopHomeViewModel homeViewModel)
    {
        TitleBarViewModel = searchViewModel;
        HomeViewModel = homeViewModel;
    }

    public ViewModelBase TitleBarViewModel { get; }
    public WorkshopHomeViewModel HomeViewModel { get; }
}