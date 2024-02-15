using System.Net.Http.Headers;
using Artemis.Core;
using Artemis.Storage.Entities.Workshop;
using Artemis.Storage.Repositories.Interfaces;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop.Handlers.UploadHandlers;
using Artemis.WebClient.Workshop.Models;
using Serilog;

namespace Artemis.WebClient.Workshop.Services;

public class WorkshopService : IWorkshopService
{
    private readonly ILogger _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IRouter _router;
    private readonly IEntryRepository _entryRepository;

    public WorkshopService(ILogger logger, IHttpClientFactory httpClientFactory, IRouter router, IEntryRepository entryRepository)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _router = router;
        _entryRepository = entryRepository;
    }

    public async Task<Stream?> GetEntryIcon(long entryId, CancellationToken cancellationToken)
    {
        HttpClient client = _httpClientFactory.CreateClient(WorkshopConstants.WORKSHOP_CLIENT_NAME);
        try
        {
            HttpResponseMessage response = await client.GetAsync($"entries/{entryId}/icon", cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync(cancellationToken);
        }
        catch (HttpRequestException)
        {
            // ignored
            return null;
        }
    }

    public async Task<ApiResult> SetEntryIcon(long entryId, Stream icon, CancellationToken cancellationToken)
    {
        icon.Seek(0, SeekOrigin.Begin);

        // Submit the archive
        HttpClient client = _httpClientFactory.CreateClient(WorkshopConstants.WORKSHOP_CLIENT_NAME);

        // Construct the request
        MultipartFormDataContent content = new();
        StreamContent streamContent = new(icon);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(streamContent, "file", "file.png");

        // Submit
        HttpResponseMessage response = await client.PostAsync($"entries/{entryId}/icon", content, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return ApiResult.FromFailure($"{response.StatusCode} - {await response.Content.ReadAsStringAsync(cancellationToken)}");
        return ApiResult.FromSuccess();
    }

    /// <inheritdoc />
    public async Task<ApiResult> UploadEntryImage(long entryId, ImageUploadRequest request, CancellationToken cancellationToken)
    {
        request.File.Seek(0, SeekOrigin.Begin);

        // Submit the archive
        HttpClient client = _httpClientFactory.CreateClient(WorkshopConstants.WORKSHOP_CLIENT_NAME);

        // Construct the request
        MultipartFormDataContent content = new();
        StreamContent streamContent = new(request.File);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(streamContent, "file", "file.png");
        content.Add(new StringContent(request.Name), "Name");
        if (request.Description != null)
            content.Add(new StringContent(request.Description), "Description");

        // Submit
        HttpResponseMessage response = await client.PostAsync($"entries/{entryId}/image", content, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return ApiResult.FromFailure($"{response.StatusCode} - {await response.Content.ReadAsStringAsync(cancellationToken)}");
        return ApiResult.FromSuccess();
    }

    /// <inheritdoc />
    public async Task DeleteEntryImage(Guid id, CancellationToken cancellationToken)
    {
        HttpClient client = _httpClientFactory.CreateClient(WorkshopConstants.WORKSHOP_CLIENT_NAME);
        HttpResponseMessage response = await client.DeleteAsync($"images/manage/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
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

    /// <inheritdoc />
    public async Task NavigateToEntry(long entryId, EntryType entryType)
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

    /// <inheritdoc />
    public List<InstalledEntry> GetInstalledEntries()
    {
        return _entryRepository.GetAll().Select(e => new InstalledEntry(e)).ToList();
    }

    /// <inheritdoc />
    public InstalledEntry? GetInstalledEntry(IEntryDetails entry)
    {
        EntryEntity? entity = _entryRepository.GetByEntryId(entry.Id);
        if (entity == null)
            return null;

        return new InstalledEntry(entity);
    }

    /// <inheritdoc />
    public void RemoveInstalledEntry(InstalledEntry installedEntry)
    {
        _entryRepository.Remove(installedEntry.Entity);
    }

    /// <inheritdoc />
    public void SaveInstalledEntry(InstalledEntry entry)
    {
        entry.Save();
        _entryRepository.Save(entry.Entity);
    }

    /// <inheritdoc />
    public void RemoveOrphanedFiles()
    {
        List<InstalledEntry> entries = GetInstalledEntries();
        foreach (string directory in Directory.GetDirectories(Constants.WorkshopFolder))
        {
            InstalledEntry? installedEntry = entries.FirstOrDefault(e => e.GetDirectory().FullName == directory);
            if (installedEntry == null)
                RemoveOrphanedDirectory(directory);
            else
            {
                DirectoryInfo currentReleaseDirectory = installedEntry.GetReleaseDirectory();
                foreach (string releaseDirectory in Directory.GetDirectories(directory))
                {
                    if (releaseDirectory != currentReleaseDirectory.FullName)
                        RemoveOrphanedDirectory(releaseDirectory);
                }
            }
        }
    }

    private void RemoveOrphanedDirectory(string directory)
    {
        _logger.Information("Removing orphaned workshop entry at {Directory}", directory);
        try
        {
            Directory.Delete(directory, true);
        }
        catch (Exception e)
        {
            _logger.Warning(e, "Failed to remove orphaned workshop entry at {Directory}", directory);
        }
    }
}