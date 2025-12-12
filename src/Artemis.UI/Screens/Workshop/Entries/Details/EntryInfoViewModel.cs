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
using Artemis.WebClient.Workshop.Extensions;
using Artemis.WebClient.Workshop.Models;
using Artemis.WebClient.Workshop.Services;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Entries.Details;

public partial class EntryInfoViewModel : ActivatableViewModelBase
{
    private readonly IRouter _router;
    private readonly INotificationService _notificationService;
    private readonly IWorkshopService _workshopService;
    [Notify] private IEntryDetails? _entry;
    [Notify] private DateTimeOffset? _updatedAt;
    [Notify] private bool _canBeManaged;

    public EntryInfoViewModel(IRouter router, INotificationService notificationService, IWorkshopService workshopService, IAuthenticationService authenticationService)
    {
        _router = router;
        _notificationService = notificationService;
        _workshopService = workshopService;

        this.WhenActivated(d =>
        {
            Observable.FromEventPattern<InstalledEntry>(x => workshopService.OnInstalledEntrySaved += x, x => workshopService.OnInstalledEntrySaved -= x)
                .StartWith([])
                .Subscribe(_ => CanBeManaged = Entry != null && Entry.EntryType != EntryType.Profile && workshopService.GetInstalledEntry(Entry.Id) != null)
                .DisposeWith(d);
        });
        
        IsAdministrator = authenticationService.GetRoles().Contains("Administrator");
    }

    public bool IsAdministrator { get; }

    public void SetEntry(IEntryDetails? entry)
    {
        Entry = entry;
        UpdatedAt = Entry != null && Entry.Releases.Any() ? Entry.Releases.Max(r => r.CreatedAt) : Entry?.CreatedAt;
        CanBeManaged = Entry != null && Entry.EntryType != EntryType.Profile && _workshopService.GetInstalledEntry(Entry.Id) != null;
    }

    public async Task CopyShareLink()
    {
        if (Entry == null)
            return;
        
        await Shared.UI.Clipboard.SetTextAsync($"{WorkshopConstants.WORKSHOP_URL}/entries/{Entry.Id}/{StringUtilities.UrlFriendly(Entry.Name)}");
        _notificationService.CreateNotification().WithTitle("Copied share link to clipboard.").Show();
    }
    
    public async Task GoToEdit()
    {
        if (Entry == null)
            return;
        
        await _router.Navigate($"workshop/library/submissions/{Entry.Id}");
    }

    public async Task GoToManage()
    {
        if (Entry == null)
            return;
        
        await _router.Navigate($"{Entry.GetEntryPath()}/manage");
    }
}