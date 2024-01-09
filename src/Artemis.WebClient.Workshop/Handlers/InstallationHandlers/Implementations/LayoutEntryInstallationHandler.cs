using System.Data;
using System.IO.Compression;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Extensions;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Utilities;
using Artemis.WebClient.Workshop.Services;

namespace Artemis.WebClient.Workshop.Handlers.InstallationHandlers;

public class LayoutEntryInstallationHandler : IEntryInstallationHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWorkshopService _workshopService;
    private readonly IDeviceService _deviceService;

    public LayoutEntryInstallationHandler(IHttpClientFactory httpClientFactory, IWorkshopService workshopService, IDeviceService deviceService)
    {
        _httpClientFactory = httpClientFactory;
        _workshopService = workshopService;
        _deviceService = deviceService;
    }

    public async Task<EntryInstallResult> InstallAsync(IGetEntryById_Entry entry, long releaseId, Progress<StreamProgress> progress, CancellationToken cancellationToken)
    {
        using MemoryStream stream = new();

        // Download the provided release
        try
        {
            HttpClient client = _httpClientFactory.CreateClient(WorkshopConstants.WORKSHOP_CLIENT_NAME);
            await client.DownloadDataAsync($"releases/download/{releaseId}", stream, progress, cancellationToken);
        }
        catch (Exception e)
        {
            return EntryInstallResult.FromFailure(e.Message);
        }

        // Ensure there is an installed entry
        InstalledEntry installedEntry = _workshopService.GetInstalledEntry(entry) ?? _workshopService.CreateInstalledEntry(entry);
        DirectoryInfo entryDirectory = installedEntry.GetDirectory();

        // If the folder already exists, remove it so that if the layout now contains less files, old things dont stick around
        if (entryDirectory.Exists)
            entryDirectory.Delete(true);
        entryDirectory.Create();

        // Extract the archive, we could go through the hoops of keeping track of progress but this should be so quick it doesn't matter
        stream.Seek(0, SeekOrigin.Begin);
        using ZipArchive archive = new(stream);
        archive.ExtractToDirectory(entryDirectory.FullName);

        ArtemisLayout layout = new(Path.Combine(entryDirectory.FullName, "layout.xml"));
        if (layout.IsValid)
            return EntryInstallResult.FromSuccess(layout);

        // If the layout ended up being invalid yoink it out again, shoooo
        entryDirectory.Delete(true);
        _workshopService.RemoveInstalledEntry(installedEntry);
        return EntryInstallResult.FromFailure("Layout failed to load because it is invalid");
    }

    public async Task<EntryUninstallResult> UninstallAsync(InstalledEntry installedEntry, CancellationToken cancellationToken)
    {
        // Remove the layout from any devices currently using it
        foreach (ArtemisDevice device in _deviceService.Devices)
        {
        }

        // Remove from filesystem
        DirectoryInfo directory = installedEntry.GetDirectory(true);
        if (directory.Exists)
            directory.Delete();

        // Remove entry
        _workshopService.RemoveInstalledEntry(installedEntry);

        return EntryUninstallResult.FromSuccess();
    }
}