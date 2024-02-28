using System.Net.Http.Headers;
using System.Net.Http.Json;
using Artemis.WebClient.Workshop.Entities;

namespace Artemis.WebClient.Workshop.Handlers.UploadHandlers;

public class PluginEntryUploadHandler : IEntryUploadHandler
{
    private readonly IHttpClientFactory _httpClientFactory;

    public PluginEntryUploadHandler(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    /// <inheritdoc />
    public async Task<EntryUploadResult> CreateReleaseAsync(long entryId, IEntrySource entrySource, CancellationToken cancellationToken)
    {
        if (entrySource is not PluginEntrySource source)
            throw new InvalidOperationException("Can only create releases for plugins");
      
        // Submit the archive
        HttpClient client = _httpClientFactory.CreateClient(WorkshopConstants.WORKSHOP_CLIENT_NAME);

        // Construct the request
        await using FileStream fileStream = File.Open(source.Path, FileMode.Open);
        MultipartFormDataContent content = new();
        StreamContent streamContent = new(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        content.Add(streamContent, "file", "file.zip");

        // Submit
        HttpResponseMessage response = await client.PostAsync("releases/upload/" + entryId, content, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return EntryUploadResult.FromFailure($"{response.StatusCode} - {await response.Content.ReadAsStringAsync(cancellationToken)}");

        Release? release = await response.Content.ReadFromJsonAsync<Release>(cancellationToken);
        return release != null ? EntryUploadResult.FromSuccess(release) : EntryUploadResult.FromFailure("Failed to deserialize response");
    }
}