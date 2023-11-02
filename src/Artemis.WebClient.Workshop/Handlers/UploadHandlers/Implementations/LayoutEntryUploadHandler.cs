using System.IO.Compression;
using Artemis.Core;
using Artemis.UI.Shared.Utilities;
using Artemis.WebClient.Workshop.Exceptions;
using RGB.NET.Layout;

namespace Artemis.WebClient.Workshop.Handlers.UploadHandlers;

public class LayoutEntryUploadHandler : IEntryUploadHandler
{
    /// <inheritdoc />
    public async Task<EntryUploadResult> CreateReleaseAsync(long entryId, IEntrySource entrySource, Progress<StreamProgress> progress, CancellationToken cancellationToken)
    {
        if (entrySource is not LayoutEntrySource source)
            throw new InvalidOperationException("Can only create releases for layouts");

        using MemoryStream archiveStream = new();
        using MemoryStream layoutStream = new();

        source.Layout.RgbLayout.Save(layoutStream);
        layoutStream.Seek(0, SeekOrigin.Begin);

        // Create an archive
        string? layoutPath = Path.GetDirectoryName(source.Layout.FilePath);
        if (layoutPath == null)
            throw new ArtemisWorkshopException($"Could not determine directory of {source.Layout.FilePath}");

        using (ZipArchive archive = new(archiveStream, ZipArchiveMode.Create, true))
        {
            // Add the layout to the archive
            ZipArchiveEntry archiveEntry = archive.CreateEntry("layout.xml");
            await using (Stream layoutArchiveStream = archiveEntry.Open())
                await layoutStream.CopyToAsync(layoutArchiveStream, cancellationToken);

            // Add the layout image to the archive
            CopyImage(layoutPath, source.Layout.LayoutCustomDeviceData.DeviceImage, archive);

            // Add the LED images to the archive
            foreach (ArtemisLedLayout ledLayout in source.Layout.Leds)
            {
                if (ledLayout.LayoutCustomLedData.LogicalLayouts == null)
                    continue;
                foreach (LayoutCustomLedDataLogicalLayout customData in ledLayout.LayoutCustomLedData.LogicalLayouts)
                    CopyImage(layoutPath, customData.Image, archive);
            }
        }

        archiveStream.Seek(0, SeekOrigin.Begin);

        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string filePath = Path.Combine(desktopPath, "layout-test.zip");
        await using (FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write))
        {
            archiveStream.WriteTo(fileStream);
        }

        return new EntryUploadResult();
    }

    private static void CopyImage(string layoutPath, string? imagePath, ZipArchive archive)
    {
        if (imagePath == null)
            return;

        string fullPath = Path.Combine(layoutPath, imagePath);
        archive.CreateEntryFromFile(fullPath, imagePath);
    }
}