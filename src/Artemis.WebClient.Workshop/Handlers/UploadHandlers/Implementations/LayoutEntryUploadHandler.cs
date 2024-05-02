using System.IO.Compression;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Artemis.Core;
using Artemis.WebClient.Workshop.Entities;
using Artemis.WebClient.Workshop.Exceptions;
using RGB.NET.Layout;

namespace Artemis.WebClient.Workshop.Handlers.UploadHandlers;

public class LayoutEntryUploadHandler : IEntryUploadHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWorkshopClient _workshopClient;

    public LayoutEntryUploadHandler(IHttpClientFactory httpClientFactory, IWorkshopClient workshopClient)
    {
        _httpClientFactory = httpClientFactory;
        _workshopClient = workshopClient;
    }

    /// <inheritdoc />
    public async Task<EntryUploadResult> CreateReleaseAsync(long entryId, IEntrySource entrySource, string? changelog, CancellationToken cancellationToken)
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

            List<string> imagePaths = [];

            // Add the layout image to the archive
            CopyImage(layoutPath, source.Layout.LayoutCustomDeviceData.DeviceImage, archive, imagePaths);

            // Add the LED images to the archive
            foreach (ArtemisLedLayout ledLayout in source.Layout.Leds)
            {
                if (ledLayout.LayoutCustomLedData.LogicalLayouts == null)
                    continue;
                foreach (LayoutCustomLedDataLogicalLayout customData in ledLayout.LayoutCustomLedData.LogicalLayouts)
                    CopyImage(layoutPath, customData.Image, archive, imagePaths);
            }
        }

        archiveStream.Seek(0, SeekOrigin.Begin);

        // Submit the archive
        HttpClient client = _httpClientFactory.CreateClient(WorkshopConstants.WORKSHOP_CLIENT_NAME);

        // Construct the request
        MultipartFormDataContent content = new();
        StreamContent streamContent = new(archiveStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        if (!string.IsNullOrWhiteSpace(changelog))
            content.Add(new StringContent(changelog), "Changelog");
        content.Add(streamContent, "file", "file.zip");

        // Submit
        HttpResponseMessage response = await client.PostAsync("releases/upload/" + entryId, content, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return EntryUploadResult.FromFailure($"{response.StatusCode} - {await response.Content.ReadAsStringAsync(cancellationToken)}");

        // Determine layout info, here we're combining user supplied data with what we can infer from the layout
        List<LayoutInfoInput> layoutInfo = GetLayoutInfoInput(source);

        // Submit layout info, overwriting the existing layout info
        await _workshopClient.SetLayoutInfo.ExecuteAsync(new SetLayoutInfoInput {EntryId = entryId, LayoutInfo = layoutInfo}, cancellationToken);

        Release? release = await response.Content.ReadFromJsonAsync<Release>(cancellationToken);
        return release != null ? EntryUploadResult.FromSuccess(release) : EntryUploadResult.FromFailure("Failed to deserialize response");
    }

    private static List<LayoutInfoInput> GetLayoutInfoInput(LayoutEntrySource source)
    {
        RGBDeviceType deviceType = Enum.Parse<RGBDeviceType>(source.Layout.RgbLayout.Type.ToString(), true);
        KeyboardLayoutType physicalLayout = Enum.Parse<KeyboardLayoutType>(source.PhysicalLayout.ToString(), true);

        List<string> logicalLayouts = source.Layout.Leds.SelectMany(l => l.GetLogicalLayoutNames()).Distinct().ToList();
        if (logicalLayouts.Any())
        {
            return logicalLayouts.SelectMany(logicalLayout => source.LayoutInfo.Select(i => new LayoutInfoInput
            {
                PhysicalLayout = deviceType == RGBDeviceType.Keyboard ? physicalLayout : null,
                LogicalLayout = logicalLayout,
                Model = i.Model,
                Vendor = i.Vendor,
                DeviceType = deviceType,
                DeviceProvider = i.DeviceProviderId
            })).ToList();
        }

        return source.LayoutInfo.Select(i => new LayoutInfoInput
        {
            PhysicalLayout = deviceType == RGBDeviceType.Keyboard ? physicalLayout : null,
            LogicalLayout = null,
            Model = i.Model,
            Vendor = i.Vendor,
            DeviceType = deviceType,
            DeviceProvider = i.DeviceProviderId
        }).ToList();
    }

    private static void CopyImage(string layoutPath, string? imagePath, ZipArchive archive, List<string> imagePaths)
    {
        if (imagePath == null || imagePaths.Contains(imagePath))
            return;

        string fullPath = Path.Combine(layoutPath, imagePath);
        archive.CreateEntryFromFile(fullPath, imagePath);
        imagePaths.Add(imagePath);
    }
}