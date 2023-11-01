using Artemis.UI.Shared.Utilities;

namespace Artemis.WebClient.Workshop.Handlers.UploadHandlers;

public interface IEntryUploadHandler
{
    Task<EntryUploadResult> CreateReleaseAsync(long entryId, IEntrySource entrySource, Progress<StreamProgress> progress, CancellationToken cancellationToken);
}