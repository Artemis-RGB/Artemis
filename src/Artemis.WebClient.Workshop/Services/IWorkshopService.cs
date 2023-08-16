using System.Net.Http.Headers;
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
    private readonly SemaphoreSlim _iconCacheLock = new(1);

    public WorkshopService(IHttpClientFactory httpClientFactory, IMainWindowService mainWindowService)
    {
        _httpClientFactory = httpClientFactory;
        mainWindowService.MainWindowClosed += (_, _) => Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await Task.Delay(1000);
            ClearCache();
        });
    }

    /// <inheritdoc />
    public async Task<Bitmap?> GetEntryIcon(Guid entryId, CancellationToken cancellationToken)
    {
        await _iconCacheLock.WaitAsync(cancellationToken);
        try
        {
            if (_entryIconCache.TryGetValue(entryId, out Stream? cachedBitmap))
            {
                cachedBitmap.Seek(0, SeekOrigin.Begin);
                return new Bitmap(cachedBitmap);
            }
        }
        finally
        {
            _iconCacheLock.Release();
        }

        HttpClient client = _httpClientFactory.CreateClient(WorkshopConstants.WORKSHOP_CLIENT_NAME);
        try
        {
            HttpResponseMessage response = await client.GetAsync($"entries/{entryId}/icon", cancellationToken);
            response.EnsureSuccessStatusCode();
            Stream data = await response.Content.ReadAsStreamAsync(cancellationToken);

            _entryIconCache[entryId] = data;
            return new Bitmap(data);
        }
        catch (HttpRequestException)
        {
            // ignored
            return null;
        }
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
    Task<Bitmap?> GetEntryIcon(Guid entryId, CancellationToken cancellationToken);
    Task<ImageUploadResult> SetEntryIcon(Guid entryId, Progress<StreamProgress> progress, Stream icon, CancellationToken cancellationToken);
}