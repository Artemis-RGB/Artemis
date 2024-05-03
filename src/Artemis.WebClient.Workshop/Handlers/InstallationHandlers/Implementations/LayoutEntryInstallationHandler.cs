using System.IO.Compression;
using Artemis.Core;
using Artemis.Core.Providers;
using Artemis.Core.Services;
using Artemis.UI.Shared.Extensions;
using Artemis.UI.Shared.Utilities;
using Artemis.WebClient.Workshop.Models;
using Artemis.WebClient.Workshop.Providers;
using Artemis.WebClient.Workshop.Services;

namespace Artemis.WebClient.Workshop.Handlers.InstallationHandlers;

public class LayoutEntryInstallationHandler : IEntryInstallationHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWorkshopService _workshopService;
    private readonly IDeviceService _deviceService;
    private readonly DefaultLayoutProvider _defaultLayoutProvider;

    public LayoutEntryInstallationHandler(IHttpClientFactory httpClientFactory, IWorkshopService workshopService, IDeviceService deviceService, DefaultLayoutProvider defaultLayoutProvider)
    {
        _httpClientFactory = httpClientFactory;
        _workshopService = workshopService;
        _deviceService = deviceService;
        _defaultLayoutProvider = defaultLayoutProvider;
    }

    public async Task<EntryInstallResult> InstallAsync(IEntrySummary entry, IRelease release, Progress<StreamProgress> progress, CancellationToken cancellationToken)
    {
        using MemoryStream stream = new();

        // Download the provided release
        try
        {
            HttpClient client = _httpClientFactory.CreateClient(WorkshopConstants.WORKSHOP_CLIENT_NAME);
            await client.DownloadDataAsync($"releases/download/{release.Id}", stream, progress, cancellationToken);
        }
        catch (Exception e)
        {
            return EntryInstallResult.FromFailure(e.Message);
        }

        // Ensure there is an installed entry
        InstalledEntry installedEntry = _workshopService.GetInstalledEntry(entry.Id) ?? new InstalledEntry(entry, release);
        DirectoryInfo releaseDirectory = installedEntry.GetReleaseDirectory(release);

        // If the folder already exists, remove it so that if the layout now contains less files, old things dont stick around
        if (releaseDirectory.Exists)
            releaseDirectory.Delete(true);
        releaseDirectory.Create();

        // Extract the archive, we could go through the hoops of keeping track of progress but this should be so quick it doesn't matter
        stream.Seek(0, SeekOrigin.Begin);
        using ZipArchive archive = new(stream);
        archive.ExtractToDirectory(releaseDirectory.FullName, true);

        ArtemisLayout layout = new(Path.Combine(releaseDirectory.FullName, "layout.xml"));
        if (layout.IsValid)
        {
            installedEntry.ApplyRelease(release);
            _workshopService.SaveInstalledEntry(installedEntry);
            return EntryInstallResult.FromSuccess(installedEntry);
        }

        // If the layout ended up being invalid yoink it out again, shoooo
        releaseDirectory.Delete(true);
        _workshopService.RemoveInstalledEntry(installedEntry);
        return EntryInstallResult.FromFailure("Layout failed to load because it is invalid");
    }

    public Task<EntryUninstallResult> UninstallAsync(InstalledEntry installedEntry, CancellationToken cancellationToken)
    {
        // Remove the layout from any devices currently using it
        foreach (ArtemisDevice device in _deviceService.Devices)
        {
            if (device.LayoutSelection.Type == WorkshopLayoutProvider.LayoutType && device.LayoutSelection.Parameter == installedEntry.EntryId.ToString())
            {
                _defaultLayoutProvider.ConfigureDevice(device);
                _deviceService.SaveDevice(device);
                _deviceService.LoadDeviceLayout(device);
            }
        }

        // Remove from filesystem
        DirectoryInfo directory = installedEntry.GetDirectory();
        if (directory.Exists)
            directory.Delete(true);

        // Remove entry
        _workshopService.RemoveInstalledEntry(installedEntry);
        return Task.FromResult(EntryUninstallResult.FromSuccess());
    }
}