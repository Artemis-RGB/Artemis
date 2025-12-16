using Artemis.UI.Shared.Routing;

namespace Artemis.UI.Screens.Workshop.Layout;

public class LayoutListViewModel : RoutableHostScreen<RoutableScreen>
{
    public override RoutableScreen DefaultScreen { get; }

    public LayoutListViewModel(LayoutListDefaultViewModel defaultViewModel)
    {
        DefaultScreen = defaultViewModel;
    }
}