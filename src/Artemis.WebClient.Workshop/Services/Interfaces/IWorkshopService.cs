using Artemis.WebClient.Workshop.Handlers.UploadHandlers;
using Artemis.WebClient.Workshop.Models;

namespace Artemis.WebClient.Workshop.Services;

public interface IWorkshopService
{
    Task<Stream?> GetEntryIcon(long entryId, CancellationToken cancellationToken);
    Task<ApiResult> SetEntryIcon(long entryId, Stream icon, CancellationToken cancellationToken);
    Task<ApiResult> UploadEntryImage(long entryId, ImageUploadRequest request, CancellationToken cancellationToken);
    Task DeleteEntryImage(Guid id, CancellationToken cancellationToken);
    Task<WorkshopStatus> GetWorkshopStatus(CancellationToken cancellationToken);
    Task<bool> ValidateWorkshopStatus(CancellationToken cancellationToken);
    Task NavigateToEntry(long entryId, EntryType entryType);

    List<InstalledEntry> GetInstalledEntries();
    InstalledEntry? GetInstalledEntry(IEntryDetails entry);
    void RemoveInstalledEntry(InstalledEntry installedEntry);
    void SaveInstalledEntry(InstalledEntry entry);
    void Initialize();

    public record WorkshopStatus(bool IsReachable, string Message);
}