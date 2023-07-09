using Artemis.UI.Shared;
using Artemis.WebClient.Workshop;

namespace Artemis.UI.Screens.Workshop.Profile;

public class ProfileListEntryViewModel : ViewModelBase
{
    public ProfileListEntryViewModel(IGetEntries_Entries_Nodes entry)
    {
        Entry = entry;
    }

    public IGetEntries_Entries_Nodes Entry { get; }
}