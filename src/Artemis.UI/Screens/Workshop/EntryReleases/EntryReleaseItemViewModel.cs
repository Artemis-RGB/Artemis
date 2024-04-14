using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.UI.Shared;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Models;
using Artemis.WebClient.Workshop.Services;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.EntryReleases;

public partial class EntryReleaseItemViewModel : ActivatableViewModelBase
{
    private readonly IWorkshopService _workshopService;
    private readonly IEntryDetails _entry;
    [Notify] private bool _isCurrentVersion;

    public EntryReleaseItemViewModel(IWorkshopService workshopService, IEntryDetails entry, IRelease release)
    {
        _workshopService = workshopService;
        _entry = entry;

        Release = release;
        UpdateIsCurrentVersion();

        this.WhenActivated(d =>
        {
            Observable.FromEventPattern<InstalledEntry>(x => _workshopService.OnInstalledEntrySaved += x, x => _workshopService.OnInstalledEntrySaved -= x)
                .Subscribe(_ => UpdateIsCurrentVersion())
                .DisposeWith(d);
        });
    }

    public IRelease Release { get; }

    private void UpdateIsCurrentVersion()
    {
        IsCurrentVersion = _workshopService.GetInstalledEntry(_entry.Id)?.ReleaseId == Release.Id;
    }
}