using Artemis.UI.Shared.Utilities;

namespace Artemis.WebClient.Workshop.UploadHandlers;

public interface IEntryUploadHandler
{
    Task<EntryUploadResult> CreateReleaseAsync(Guid entryId, object file, Progress<StreamProgress> progress, CancellationToken cancellationToken);
}