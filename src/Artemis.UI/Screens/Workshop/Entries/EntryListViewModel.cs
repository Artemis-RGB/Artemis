using System;
using System.Reactive;
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

public class EntryListViewModel : ActivatableViewModelBase
{
    private readonly IRouter _router;
    private readonly ObservableAsPropertyHelper<Bitmap?> _entryIcon;

    public EntryListViewModel(IGetEntries_Entries_Items entry, IRouter router, IWorkshopService workshopService)
    {
        _router = router;

        Entry = entry;
        EntryIcon = workshopService.GetEntryIcon(entry.Id, CancellationToken.None);
        NavigateToEntry = ReactiveCommand.CreateFromTask(ExecuteNavigateToEntry);
    }

    public IGetEntries_Entries_Items Entry { get; }
    public Task<Bitmap?> EntryIcon { get; }
    public ReactiveCommand<Unit, Unit> NavigateToEntry { get; }

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