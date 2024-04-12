using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Extensions;
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

        this.WhenActivated(d =>
        {
            router.CurrentPath.Subscribe(p =>
                    SelectedRelease = p != null && p.StartsWith(Entry.GetEntryPath()) && float.TryParse(p.Split('/').Last(), out float releaseId)
                        ? Releases.FirstOrDefault(r => r.Release.Id == releaseId)
                        : null)
                .DisposeWith(d);

            this.WhenAnyValue(vm => vm.SelectedRelease)
                .WhereNotNull()
                .Subscribe(s => _router.Navigate($"{Entry.GetEntryPath()}/releases/{s.Release.Id}"))
                .DisposeWith(d);
        });
    }

    public IEntryDetails Entry { get; }
    public List<EntryReleaseItemViewModel> Releases { get; }
}