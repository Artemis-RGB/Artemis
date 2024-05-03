using Artemis.UI.Screens.Workshop.Entries.List;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;

namespace Artemis.UI.Screens.Workshop.Layout;

public class LayoutListViewModel : RoutableHostScreen<RoutableScreen>
{
    private readonly EntryListViewModel _entryListViewModel;
    public override RoutableScreen DefaultScreen { get; }

    public LayoutListViewModel(LayoutListDefaultViewModel defaultViewModel)
    {
        DefaultScreen = defaultViewModel;
    }
}