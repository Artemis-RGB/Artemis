using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Models;
using Artemis.WebClient.Workshop.Services;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Entries.List;

public partial class EntryListItemViewModel : ActivatableViewModelBase
{
    private readonly IRouter _router;
    [Notify] private bool _isInstalled;
    [Notify] private bool _updateAvailable;

    public EntryListItemViewModel(IEntrySummary entry, IRouter router, IWorkshopService workshopService, Func<IEntrySummary, EntryVoteViewModel> getEntryVoteViewModel)
    {
        _router = router;

        Entry = entry;
        VoteViewModel = getEntryVoteViewModel(entry);

        this.WhenActivated((CompositeDisposable _) =>
        {
            InstalledEntry? installedEntry = workshopService.GetInstalledEntry(entry.Id);
            IsInstalled = installedEntry != null;
            UpdateAvailable = installedEntry != null && installedEntry.ReleaseId != entry.LatestReleaseId;
        });
    }

    public IEntrySummary Entry { get; }
    public EntryVoteViewModel VoteViewModel { get; }

    public async Task NavigateToEntry()
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
                await _router.Navigate($"workshop/entries/plugins/details/{Entry.Id}");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}