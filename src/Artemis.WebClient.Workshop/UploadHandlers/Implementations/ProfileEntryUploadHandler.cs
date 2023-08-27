using System.Net.Http.Headers;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.Storage.Repositories.Interfaces;
using Artemis.UI.Shared.Utilities;
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
    public async Task<EntryUploadResult> CreateReleaseAsync(Guid entryId, object file, Progress<StreamProgress> progress, CancellationToken cancellationToken)
    {
        if (file is not ProfileConfiguration profileConfiguration)
            throw new InvalidOperationException("Can only create releases for profile configurations");

        await using Stream archiveStream = await _profileService.ExportProfile(profileConfiguration);

        // Submit the archive
        HttpClient client = _httpClientFactory.CreateClient(WorkshopConstants.WORKSHOP_CLIENT_NAME);

        // Construct the request
        MultipartFormDataContent content = new();
        ProgressableStreamContent streamContent = new(archiveStream, progress);
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