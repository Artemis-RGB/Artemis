
namespace Artemis.WebClient.Workshop.UploadHandlers;

public interface IEntryUploadHandler
{
    Task<EntryUploadResult> CreateReleaseAsync(Guid entryId, object file, CancellationToken cancellationToken);
}