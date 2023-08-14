using Artemis.UI.Shared.Utilities;

namespace Artemis.WebClient.Workshop.UploadHandlers.Implementations;

public class LayoutEntryUploadHandler : IEntryUploadHandler
{
    /// <inheritdoc />
    public async Task<EntryUploadResult> CreateReleaseAsync(Guid entryId, object file, Progress<StreamProgress> progress, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}