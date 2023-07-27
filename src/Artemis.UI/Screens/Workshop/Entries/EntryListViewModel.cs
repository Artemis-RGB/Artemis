using System;
using System.Reactive;
using System.Threading.Tasks;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Entries;

public class EntryListViewModel : ViewModelBase
{
    private readonly IRouter _router;

    public EntryListViewModel(IGetEntries_Entries_Items entry, IRouter router)
    {
        _router = router;
        Entry = entry;
        NavigateToEntry = ReactiveCommand.CreateFromTask(ExecuteNavigateToEntry);
    }

    public IGetEntries_Entries_Items Entry { get; }
    public ReactiveCommand<Unit,Unit> NavigateToEntry { get; }

    private async Task ExecuteNavigateToEntry()
    {
        switch (Entry.EntryType)
        {
            case EntryType.Layout:
                await _router.Navigate($"workshop/layouts/{Entry.Id}");
                break;
            case EntryType.Profile:
                await _router.Navigate($"workshop/profiles/{Entry.Id}");
                break;
            case EntryType.Plugin:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}