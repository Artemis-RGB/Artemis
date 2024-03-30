using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Models;
using Artemis.WebClient.Workshop.Services;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Entries.Details;

public partial class EntryInfoViewModel : ActivatableViewModelBase
{
    private readonly IRouter _router;
    private readonly INotificationService _notificationService;
    [Notify] private bool _canBeManaged;

    public EntryInfoViewModel(IEntryDetails entry, IRouter router, INotificationService notificationService, IWorkshopService workshopService)
    {
        _router = router;
        _notificationService = notificationService;
        Entry = entry;
        UpdatedAt = Entry.Releases.Any() ? Entry.Releases.Max(r => r.CreatedAt) : Entry.CreatedAt;
        CanBeManaged = Entry.EntryType != EntryType.Profile && workshopService.GetInstalledEntry(entry.Id) != null;

        this.WhenActivated(d =>
        {
            Observable.FromEventPattern<InstalledEntry>(x => workshopService.OnInstalledEntrySaved += x, x => workshopService.OnInstalledEntrySaved -= x)
                .StartWith([])
                .Subscribe(_ => CanBeManaged = Entry.EntryType != EntryType.Profile && workshopService.GetInstalledEntry(entry.Id) != null)
                .DisposeWith(d);
        });
    }

    public IEntryDetails Entry { get; }
    public DateTimeOffset? UpdatedAt { get; }

    public async Task CopyShareLink()
    {
        await Shared.UI.Clipboard.SetTextAsync($"{WorkshopConstants.WORKSHOP_URL}/entries/{Entry.Id}/{StringUtilities.UrlFriendly(Entry.Name)}");
        _notificationService.CreateNotification().WithTitle("Copied share link to clipboard.").Show();
    }

    public async Task GoToManage()
    {
        await _router.Navigate("/manage");
    }
}