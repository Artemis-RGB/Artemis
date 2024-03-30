using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Models;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.EntryReleases;

public partial class EntryReleasesViewModel : ActivatableViewModelBase
{
    private readonly IRouter _router;
    [Notify] private EntryReleaseItemViewModel? _selectedRelease;

    public EntryReleasesViewModel(IEntryDetails entry, IRouter router, Func<IRelease, EntryReleaseItemViewModel> getEntryReleaseItemViewModel)
    {
        _router = router;

        Entry = entry;
        Releases = Entry.Releases.OrderByDescending(r => r.CreatedAt).Take(5).Select(r => getEntryReleaseItemViewModel(r)).ToList();
        NavigateToRelease = ReactiveCommand.CreateFromTask<IRelease>(ExecuteNavigateToRelease);

        this.WhenActivated(d =>
        {
            router.CurrentPath.Subscribe(p => SelectedRelease = p != null && p.Contains("releases") && float.TryParse(p.Split('/').Last(), out float releaseId)
                    ? Releases.FirstOrDefault(r => r.Release.Id == releaseId)
                    : null)
                .DisposeWith(d);

            this.WhenAnyValue(vm => vm.SelectedRelease)
                .WhereNotNull()
                .Subscribe(s => ExecuteNavigateToRelease(s.Release))
                .DisposeWith(d);
        });
    }

    public IEntryDetails Entry { get; }
    public List<EntryReleaseItemViewModel> Releases { get; }
    public ReactiveCommand<IRelease, Unit> NavigateToRelease { get; }

    private async Task ExecuteNavigateToRelease(IRelease release)
    {
        switch (Entry.EntryType)
        {
            case EntryType.Profile:
                await _router.Navigate($"workshop/entries/profiles/details/{Entry.Id}/releases/{release.Id}");
                break;
            case EntryType.Layout:
                await _router.Navigate($"workshop/entries/layouts/details/{Entry.Id}/releases/{release.Id}");
                break;
            case EntryType.Plugin:
                await _router.Navigate($"workshop/entries/plugins/details/{Entry.Id}/releases/{release.Id}");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(Entry.EntryType));
        }
    }
}