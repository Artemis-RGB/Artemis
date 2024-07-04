using System.Net.Http.Headers;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.Storage.Entities.Workshop;
using Artemis.Storage.Repositories.Interfaces;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Utilities;
using Artemis.WebClient.Workshop.Exceptions;
using Artemis.WebClient.Workshop.Handlers.InstallationHandlers;
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
    private readonly Lazy<IPluginManagementService> _pluginManagementService;
    private readonly Lazy<IProfileService> _profileService;
    private readonly EntryInstallationHandlerFactory _factory;
    private bool _initialized;

    public WorkshopService(ILogger logger,
        IHttpClientFactory httpClientFactory,
        IRouter router,
        IEntryRepository entryRepository,
        Lazy<IPluginManagementService> pluginManagementService,
        Lazy<IProfileService> profileService,
        EntryInstallationHandlerFactory factory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _router = router;
        _entryRepository = entryRepository;
        _pluginManagementService = pluginManagementService;
        _profileService = profileService;
        _factory = factory;
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
    public async Task<EntryInstallResult> InstallEntry(IEntrySummary entry, IRelease release, Progress<StreamProgress> progress, CancellationToken cancellationToken)
    {
        IEntryInstallationHandler handler = _factory.CreateHandler(entry.EntryType);
        EntryInstallResult result = await handler.InstallAsync(entry, release, progress, cancellationToken);
        if (result.IsSuccess && result.Entry != null)
            OnEntryInstalled?.Invoke(this, result.Entry);
        else
            _logger.Warning("Failed to install entry {EntryId}: {Message}", entry.Id, result.Message);
        
        return result;
    }

    /// <inheritdoc />
    public async Task<EntryUninstallResult> UninstallEntry(InstalledEntry installedEntry, CancellationToken cancellationToken)
    {
        IEntryInstallationHandler handler = _factory.CreateHandler(installedEntry.EntryType);
        EntryUninstallResult result = await handler.UninstallAsync(installedEntry, cancellationToken);
        if (result.IsSuccess)
            OnEntryUninstalled?.Invoke(this, installedEntry);
        else
            _logger.Warning("Failed to uninstall entry {EntryId}: {Message}", installedEntry.EntryId, result.Message);

        return result;
    }

    /// <inheritdoc />
    public List<InstalledEntry> GetInstalledEntries()
    {
        return _entryRepository.GetAll().Select(e => new InstalledEntry(e)).ToList();
    }

    /// <inheritdoc />
    public InstalledEntry? GetInstalledEntry(long entryId)
    {
        EntryEntity? entity = _entryRepository.GetByEntryId(entryId);
        if (entity == null)
            return null;

        return new InstalledEntry(entity);
    }

    /// <inheritdoc />
    public InstalledEntry? GetInstalledEntryByPlugin(Plugin plugin)
    {
        return GetInstalledEntries().FirstOrDefault(e => e.TryGetMetadata("PluginId", out Guid pluginId) && pluginId == plugin.Guid);
    }

    /// <inheritdoc />
    public InstalledEntry? GetInstalledEntryByProfile(ProfileConfiguration profileConfiguration)
    {
        return GetInstalledEntries().FirstOrDefault(e => e.TryGetMetadata("ProfileId", out Guid pluginId) && pluginId == profileConfiguration.ProfileId);
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

        OnInstalledEntrySaved?.Invoke(this, entry);
    }

    /// <inheritdoc />
    public void Initialize()
    {
        if (_initialized)
            throw new ArtemisWorkshopException("Workshop service is already initialized");

        try
        {
            if (!Directory.Exists(Constants.WorkshopFolder))
                Directory.CreateDirectory(Constants.WorkshopFolder);

            RemoveOrphanedFiles();

            _pluginManagementService.Value.AdditionalPluginDirectories.AddRange(GetInstalledEntries()
                .Where(e => e.EntryType == EntryType.Plugin)
                .Select(e => e.GetReleaseDirectory()));

            _pluginManagementService.Value.PluginRemoved += PluginManagementServiceOnPluginRemoved;
            _profileService.Value.ProfileRemoved += ProfileServiceOnProfileRemoved;

            _initialized = true;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to initialize workshop service");
        }
    }

    private void RemoveOrphanedFiles()
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

    private void ProfileServiceOnProfileRemoved(object? sender, ProfileConfigurationEventArgs e)
    {
        InstalledEntry? entry = GetInstalledEntryByProfile(e.ProfileConfiguration);
        if (entry == null)
            return;

        _logger.Information("Profile {Profile} was removed, uninstalling entry", e.ProfileConfiguration);
        Task.Run(() => UninstallEntry(entry, CancellationToken.None));
    }

    private void PluginManagementServiceOnPluginRemoved(object? sender, PluginEventArgs e)
    {
        InstalledEntry? entry = GetInstalledEntryByPlugin(e.Plugin);
        if (entry == null)
            return;

        _logger.Information("Plugin {Plugin} was removed, uninstalling entry", e.Plugin);
        Task.Run(() => UninstallEntry(entry, CancellationToken.None));
    }

    public event EventHandler<InstalledEntry>? OnInstalledEntrySaved;
    public event EventHandler<InstalledEntry>? OnEntryUninstalled;
    public event EventHandler<InstalledEntry>? OnEntryInstalled;
}