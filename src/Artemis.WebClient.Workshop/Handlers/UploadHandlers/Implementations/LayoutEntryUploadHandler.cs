using System.Xml.Serialization;
using Artemis.UI.Shared.Utilities;
using RGB.NET.Layout;

namespace Artemis.WebClient.Workshop.Handlers.UploadHandlers;

public class LayoutEntryUploadHandler : IEntryUploadHandler
{
    /// <inheritdoc />
    public async Task<EntryUploadResult> CreateReleaseAsync(long entryId, IEntrySource entrySource, Progress<StreamProgress> progress, CancellationToken cancellationToken)
    {
        if (entrySource is not LayoutEntrySource source)
            throw new InvalidOperationException("Can only create releases for layouts");
        
        // Create a copy of the layout, image paths are about to be rewritten
        XmlSerializer serializer = new(typeof(DeviceLayout));
        using MemoryStream ms = new();
        await using StreamWriter writer = new(ms);
        serializer.Serialize(writer, source.Layout.RgbLayout);
        await writer.FlushAsync();
        ms.Seek(0, SeekOrigin.Begin);

        return new EntryUploadResult();
    }
}