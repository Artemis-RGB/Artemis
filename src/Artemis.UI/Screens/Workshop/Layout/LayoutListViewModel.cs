using Artemis.UI.Screens.Workshop.Entries.List;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;

namespace Artemis.UI.Screens.Workshop.Layout;

public class LayoutListViewModel : RoutableHostScreen<RoutableScreen>
{
    public EntryListViewModel EntryListViewModel { get; }

    public LayoutListViewModel(EntryListViewModel entryListViewModel)
    {
        EntryListViewModel = entryListViewModel;
        EntryListViewModel.EntryType = EntryType.Layout;
    }
}