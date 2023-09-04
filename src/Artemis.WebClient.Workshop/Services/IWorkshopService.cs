using System.Net.Http.Headers;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Utilities;
using Artemis.WebClient.Workshop.UploadHandlers;

namespace Artemis.WebClient.Workshop.Services;

public class WorkshopService : IWorkshopService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IRouter _router;

    public WorkshopService(IHttpClientFactory httpClientFactory, IRouter router)
    {
        _httpClientFactory = httpClientFactory;
        _router = router;
    }

    public async Task<ImageUploadResult> SetEntryIcon(Guid entryId, Progress<StreamProgress> progress, Stream icon, CancellationToken cancellationToken)
    {
        icon.Seek(0, SeekOrigin.Begin);

        // Submit the archive
        HttpClient client = _httpClientFactory.CreateClient(WorkshopConstants.WORKSHOP_CLIENT_NAME);

        // Construct the request
        MultipartFormDataContent content = new();
        ProgressableStreamContent streamContent = new(icon, progress);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(streamContent, "file", "file.png");

        // Submit
        HttpResponseMessage response = await client.PostAsync($"entries/{entryId}/icon", content, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return ImageUploadResult.FromFailure($"{response.StatusCode} - {await response.Content.ReadAsStringAsync(cancellationToken)}");
        return ImageUploadResult.FromSuccess();
    }

    /// <inheritdoc />
    public async Task<IWorkshopService.WorkshopStatus> GetWorkshopStatus(CancellationToken cancellationToken)
    {
        try
        {
            // Don't use the workshop client which adds auth headers
            HttpClient client = _httpClientFactory.CreateClient();
            HttpResponseMessage response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, WorkshopConstants.WORKSHOP_URL + "/status"), cancellationToken);
            return new IWorkshopService.WorkshopStatus(response.IsSuccessStatusCode, response.StatusCode.ToString());
        }
        catch (OperationCanceledException e)
        {
            return new IWorkshopService.WorkshopStatus(false, e.Message);
        }
        catch (HttpRequestException e)
        {
            return new IWorkshopService.WorkshopStatus(false, e.Message);
        }
    }
    
    /// <inheritdoc />
    public async Task<bool> ValidateWorkshopStatus(CancellationToken cancellationToken)
    {
        IWorkshopService.WorkshopStatus status = await GetWorkshopStatus(cancellationToken);
        if (!status.IsReachable && !cancellationToken.IsCancellationRequested)
            await _router.Navigate($"workshop/offline/{status.Message}");
        return status.IsReachable;
    }

    public async Task NavigateToEntry(Guid entryId, EntryType entryType)
    {
        switch (entryType)
        {
            case EntryType.Profile:
                await _router.Navigate($"workshop/entries/profiles/details/{entryId}");
                break;
            case EntryType.Layout:
                await _router.Navigate($"workshop/entries/layouts/details/{entryId}");
                break;
            case EntryType.Plugin:
                await _router.Navigate($"workshop/entries/plugins/details/{entryId}");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(entryType));
        }
    }
}

public interface IWorkshopService
{
    Task<ImageUploadResult> SetEntryIcon(Guid entryId, Progress<StreamProgress> progress, Stream icon, CancellationToken cancellationToken);
    Task<WorkshopStatus> GetWorkshopStatus(CancellationToken cancellationToken);
    Task<bool> ValidateWorkshopStatus(CancellationToken cancellationToken);
    Task NavigateToEntry(Guid entryId, EntryType entryType);
    
    public record WorkshopStatus(bool IsReachable, string Message);
}