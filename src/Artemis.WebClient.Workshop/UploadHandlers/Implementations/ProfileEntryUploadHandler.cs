using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.Web.Workshop.Entities;
using Newtonsoft.Json;

namespace Artemis.WebClient.Workshop.UploadHandlers.Implementations;

public class ProfileEntryUploadHandler : IEntryUploadHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IProfileService _profileService;

    public ProfileEntryUploadHandler(IHttpClientFactory httpClientFactory, IProfileService profileService)
    {
        _httpClientFactory = httpClientFactory;
        _profileService = profileService;
    }

    /// <inheritdoc />
    public async Task<EntryUploadResult> CreateReleaseAsync(Guid entryId, object file, CancellationToken cancellationToken)
    {
        if (file is not ProfileConfiguration profileConfiguration)
            throw new InvalidOperationException("Can only create releases for profile configurations");

        ProfileConfigurationExportModel export = _profileService.ExportProfile(profileConfiguration);
        string json = JsonConvert.SerializeObject(export, IProfileService.ExportSettings);

        using MemoryStream archiveStream = new();

        // Create a ZIP archive with a single entry on the archive stream
        using (ZipArchive archive = new(archiveStream, ZipArchiveMode.Create, true))
        {
            ZipArchiveEntry entry = archive.CreateEntry("profile.json");
            await using (Stream entryStream = entry.Open())
            {
                await entryStream.WriteAsync(Encoding.Default.GetBytes(json), cancellationToken);
            }
        }

        // Submit the archive
        HttpClient client = _httpClientFactory.CreateClient(WorkshopConstants.WORKSHOP_CLIENT_NAME);
        
        // Construct the request
        archiveStream.Seek(0, SeekOrigin.Begin);
        MultipartFormDataContent content = new();
        StreamContent streamContent = new(archiveStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        content.Add(streamContent, "file", "file.zip");
        
        // Submit
        HttpResponseMessage response = await client.PostAsync("releases/upload/" + entryId, content, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return EntryUploadResult.FromFailure($"{response.StatusCode} - {await response.Content.ReadAsStringAsync(cancellationToken)}");
        
        Release? release = JsonConvert.DeserializeObject<Release>(await response.Content.ReadAsStringAsync(cancellationToken));
        return release != null ? EntryUploadResult.FromSuccess(release) : EntryUploadResult.FromFailure("Failed to deserialize response");
    }
}