using System;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;

namespace Artemis.UI.Screens.Workshop.Entries.Details;

public class EntryInfoViewModel : ViewModelBase
{
    private readonly INotificationService _notificationService;
    public IGetEntryById_Entry Entry { get; }
    public DateTimeOffset? UpdatedAt { get; }
    
    public EntryInfoViewModel(IGetEntryById_Entry entry, INotificationService notificationService)
    {
        _notificationService = notificationService;
        Entry = entry;
        UpdatedAt = Entry.LatestRelease?.CreatedAt ?? Entry.CreatedAt;
    }

    public async Task CopyShareLink()
    {
        await Shared.UI.Clipboard.SetTextAsync($"{WorkshopConstants.WORKSHOP_URL}/entries/{Entry.Id}/{StringUtilities.UrlFriendly(Entry.Name)}");
        _notificationService.CreateNotification().WithTitle("Copied share link to clipboard.").Show();
    }
}