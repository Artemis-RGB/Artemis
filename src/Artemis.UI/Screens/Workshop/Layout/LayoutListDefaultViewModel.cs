using System;
using Artemis.UI.Screens.Workshop.Entries.List;
using Artemis.UI.Screens.Workshop.LayoutFinder;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;

namespace Artemis.UI.Screens.Workshop.Layout;

public class LayoutListDefaultViewModel : RoutableScreen
{
    public LayoutListDefaultViewModel(LayoutFinderViewModel layoutFinderViewModel, Func<EntryType, EntryListViewModel> getEntryListViewModel)
    {
        LayoutFinderViewModel = layoutFinderViewModel;
        EntryListViewModel = getEntryListViewModel(EntryType.Layout);
        EntryListViewModel.ShowCategoryFilter = false;
    }

    public LayoutFinderViewModel LayoutFinderViewModel { get; }
    public EntryListViewModel EntryListViewModel { get; }
}