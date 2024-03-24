using Artemis.UI.Screens.Workshop.Entries.List;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;

namespace Artemis.UI.Screens.Workshop.Profile;

public class ProfileListViewModel : RoutableHostScreen<RoutableScreen>
{
    public EntryListViewModel EntryListViewModel { get; }

    public ProfileListViewModel(EntryListViewModel entryListViewModel)
    {
        EntryListViewModel = entryListViewModel;
        EntryListViewModel.EntryType = EntryType.Profile;
    }
}