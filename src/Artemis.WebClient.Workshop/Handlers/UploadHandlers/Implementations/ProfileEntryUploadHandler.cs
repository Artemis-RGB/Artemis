using System.Net.Http.Headers;
using System.Net.Http.Json;
using Artemis.Core.Services;
using Artemis.WebClient.Workshop.Entities;

namespace Artemis.WebClient.Workshop.Handlers.UploadHandlers;

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
    public async Task<EntryUploadResult> CreateReleaseAsync(long entryId, IEntrySource entrySource, CancellationToken cancellationToken)
    {
        if (entrySource is not ProfileEntrySource source)
            throw new InvalidOperationException("Can only create releases for profile configurations");

        await using Stream archiveStream = await _profileService.ExportProfile(source.ProfileConfiguration);

        // Submit the archive
        HttpClient client = _httpClientFactory.CreateClient(WorkshopConstants.WORKSHOP_CLIENT_NAME);

        // Construct the request
        MultipartFormDataContent content = new();
        StreamContent streamContent = new(archiveStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        content.Add(JsonContent.Create(source.Dependencies.Select(d => new {PluginId = d.Plugin.Guid, FeatureId = d.Id}).ToList()), "ReleaseDependencies");
        content.Add(streamContent, "file", "file.zip");

        // Submit
        HttpResponseMessage response = await client.PostAsync("releases/upload/" + entryId, content, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return EntryUploadResult.FromFailure($"{response.StatusCode} - {await response.Content.ReadAsStringAsync(cancellationToken)}");

        Release? release = await response.Content.ReadFromJsonAsync<Release>(cancellationToken);
        return release != null ? EntryUploadResult.FromSuccess(release) : EntryUploadResult.FromFailure("Failed to deserialize response");
    }
}