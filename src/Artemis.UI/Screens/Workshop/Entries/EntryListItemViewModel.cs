using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Services;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Entries;

public class EntryListItemViewModel : ActivatableViewModelBase
{
    private readonly IRouter _router;

    public EntryListItemViewModel(IGetEntries_Entries_Items entry, IRouter router)
    {
        _router = router;

        Entry = entry;
        NavigateToEntry = ReactiveCommand.CreateFromTask(ExecuteNavigateToEntry);
    }

    public IGetEntries_Entries_Items Entry { get; }
    public ReactiveCommand<Unit, Unit> NavigateToEntry { get; }

    private async Task ExecuteNavigateToEntry()
    {
        switch (Entry.EntryType)
        {
            case EntryType.Layout:
                await _router.Navigate($"workshop/entries/layouts/details/{Entry.Id}");
                break;
            case EntryType.Profile:
                await _router.Navigate($"workshop/entries/profiles/details/{Entry.Id}");
                break;
            case EntryType.Plugin:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}