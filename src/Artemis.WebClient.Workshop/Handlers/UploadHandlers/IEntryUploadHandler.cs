namespace Artemis.WebClient.Workshop.Handlers.UploadHandlers;

public interface IEntryUploadHandler
{
    Task<EntryUploadResult> CreateReleaseAsync(long entryId, IEntrySource entrySource, string? changelog, CancellationToken cancellationToken);
}