using System.Net;
using System.Net.Http.Headers;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services.MainWindow;
using Artemis.UI.Shared.Utilities;
using Artemis.WebClient.Workshop.UploadHandlers;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace Artemis.WebClient.Workshop.Services;

public class WorkshopService : IWorkshopService
{
    private readonly Dictionary<Guid, Stream> _entryIconCache = new();
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IRouter _router;
    private readonly SemaphoreSlim _iconCacheLock = new(1);

    public WorkshopService(IHttpClientFactory httpClientFactory, IMainWindowService mainWindowService, IRouter router)
    {
        _httpClientFactory = httpClientFactory;
        _router = router;
        mainWindowService.MainWindowClosed += (_, _) => Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await Task.Delay(1000);
            ClearCache();
        });
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
    public async Task<IWorkshopService.WorkshopStatus> GetWorkshopStatus()
    {
        try
        {
            // Don't use the workshop client which adds auth headers
            HttpClient client = _httpClientFactory.CreateClient();
            HttpResponseMessage response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, WorkshopConstants.WORKSHOP_URL + "/status"));
            return new IWorkshopService.WorkshopStatus(response.IsSuccessStatusCode, response.StatusCode.ToString());
        }
        catch (HttpRequestException e)
        {
            return new IWorkshopService.WorkshopStatus(false, e.Message);
        }
    }

    /// <inheritdoc />
    public async Task<bool> ValidateWorkshopStatus()
    {
        IWorkshopService.WorkshopStatus status = await GetWorkshopStatus();
        if (!status.IsReachable)
            await _router.Navigate($"workshop/offline/{status.Message}");
        return status.IsReachable;
    }

    private void ClearCache()
    {
        try
        {
            List<Stream> values = _entryIconCache.Values.ToList();
            _entryIconCache.Clear();
            foreach (Stream bitmap in values)
                bitmap.Dispose();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}

public interface IWorkshopService
{
    Task<ImageUploadResult> SetEntryIcon(Guid entryId, Progress<StreamProgress> progress, Stream icon, CancellationToken cancellationToken);
    Task<WorkshopStatus> GetWorkshopStatus();
    Task<bool> ValidateWorkshopStatus();

    public record WorkshopStatus(bool IsReachable, string Message);
}