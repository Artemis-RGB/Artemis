using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Shared;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Services;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Search;

public class SearchResultViewModel : ActivatableViewModelBase
{
    private readonly IWorkshopService _workshopService;
    private ObservableAsPropertyHelper<Bitmap?>? _entryIcon;

    public SearchResultViewModel(ISearchEntries_SearchEntries entry, IWorkshopService workshopService)
    {
        _workshopService = workshopService;

        Entry = entry;
        this.WhenActivated(d =>
        {
            _entryIcon = Observable.FromAsync(c => _workshopService.GetEntryIcon(Entry.Id, c)).ToProperty(this, vm => vm.EntryIcon);
            _entryIcon.DisposeWith(d);
        });
    }

    public ISearchEntries_SearchEntries Entry { get; }
    public Bitmap? EntryIcon => _entryIcon?.Value;
}