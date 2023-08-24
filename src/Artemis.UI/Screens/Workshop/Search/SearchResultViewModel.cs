using Artemis.UI.Shared;
using Artemis.WebClient.Workshop;

namespace Artemis.UI.Screens.Workshop.Search;

public class SearchResultViewModel : ActivatableViewModelBase
{
    public SearchResultViewModel(ISearchEntries_SearchEntries entry)
    {
        Entry = entry;
    }

    public ISearchEntries_SearchEntries Entry { get; }
}