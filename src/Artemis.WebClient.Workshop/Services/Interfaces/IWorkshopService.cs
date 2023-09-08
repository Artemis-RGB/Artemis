using Artemis.UI.Shared.Utilities;
using Artemis.WebClient.Workshop.Handlers.UploadHandlers;

namespace Artemis.WebClient.Workshop.Services;

public interface IWorkshopService
{
    Task<Stream?> GetEntryIcon(Guid entryId, CancellationToken cancellationToken);
    Task<ImageUploadResult> SetEntryIcon(Guid entryId, Progress<StreamProgress> progress, Stream icon, CancellationToken cancellationToken);
    Task<WorkshopStatus> GetWorkshopStatus(CancellationToken cancellationToken);
    Task<bool> ValidateWorkshopStatus(CancellationToken cancellationToken);
    Task NavigateToEntry(Guid entryId, EntryType entryType);

    List<InstalledEntry> GetInstalledEntries();
    InstalledEntry? GetInstalledEntry(IGetEntryById_Entry entry);
    InstalledEntry CreateInstalledEntry(IGetEntryById_Entry entry);
    void RemoveInstalledEntry(InstalledEntry installedEntry);
    void SaveInstalledEntry(InstalledEntry entry);


    public record WorkshopStatus(bool IsReachable, string Message);
}