using System;
using Artemis.UI.Screens.Workshop.Entries.List;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;

namespace Artemis.UI.Screens.Workshop.Profile;

public class ProfileListViewModel : RoutableHostScreen<RoutableScreen>
{
    private readonly EntryListViewModel _entryListViewModel;
    public override RoutableScreen DefaultScreen => _entryListViewModel;

    public ProfileListViewModel(Func<EntryType, EntryListViewModel> getEntryListViewModel)
    {
        _entryListViewModel = getEntryListViewModel(EntryType.Profile);
    }
}