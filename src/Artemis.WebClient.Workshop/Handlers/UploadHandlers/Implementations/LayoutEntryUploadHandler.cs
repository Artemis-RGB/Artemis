using Artemis.UI.Shared.Utilities;

namespace Artemis.WebClient.Workshop.Handlers.UploadHandlers.Implementations;

public class LayoutEntryUploadHandler : IEntryUploadHandler
{
    /// <inheritdoc />
    public async Task<EntryUploadResult> CreateReleaseAsync(long entryId, object file, Progress<StreamProgress> progress, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}