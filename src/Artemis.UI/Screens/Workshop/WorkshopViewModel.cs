using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;

namespace Artemis.UI.Screens.Workshop;

public class WorkshopViewModel : RoutableScreen<object>, IMainScreenViewModel
{
    public WorkshopViewModel()
    {
    }

    public ViewModelBase? TitleBarViewModel => null;
}