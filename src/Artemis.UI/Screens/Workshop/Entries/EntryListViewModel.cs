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

public class EntryListViewModel : ActivatableViewModelBase
{
    private readonly IRouter _router;
    private readonly IWorkshopService _workshopService;
    private ObservableAsPropertyHelper<Bitmap?>? _entryIcon;

    public EntryListViewModel(IGetEntries_Entries_Items entry, IRouter router, IWorkshopService workshopService)
    {
        _router = router;
        _workshopService = workshopService;

        Entry = entry;
        NavigateToEntry = ReactiveCommand.CreateFromTask(ExecuteNavigateToEntry);
        
        this.WhenActivated(d =>
        {
            _entryIcon = Observable.FromAsync(GetIcon).ToProperty(this, vm => vm.EntryIcon);
            _entryIcon.DisposeWith(d);
        });
    }

    public IGetEntries_Entries_Items Entry { get; }
    public Bitmap? EntryIcon => _entryIcon?.Value;
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
    
    private async Task<Bitmap?> GetIcon(CancellationToken cancellationToken)
    {
        // Take at least 100ms to allow the UI to load and make the whole thing smooth
        Task<Bitmap?> iconTask = _workshopService.GetEntryIcon(Entry.Id, cancellationToken);
        await Task.Delay(100, cancellationToken);
        return await iconTask;
    }
}