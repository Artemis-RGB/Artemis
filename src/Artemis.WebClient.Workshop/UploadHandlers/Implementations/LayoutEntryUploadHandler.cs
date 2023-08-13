using RGB.NET.Layout;

namespace Artemis.WebClient.Workshop.UploadHandlers.Implementations;

public class LayoutEntryUploadHandler : IEntryUploadHandler
{
    /// <inheritdoc />
    public async Task<EntryUploadResult> CreateReleaseAsync(Guid entryId, object file, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}