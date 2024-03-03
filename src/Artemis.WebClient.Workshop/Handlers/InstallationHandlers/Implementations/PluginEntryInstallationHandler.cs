using System.IO.Compression;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Extensions;
using Artemis.UI.Shared.Utilities;
using Artemis.WebClient.Workshop.Exceptions;
using Artemis.WebClient.Workshop.Models;
using Artemis.WebClient.Workshop.Services;

namespace Artemis.WebClient.Workshop.Handlers.InstallationHandlers;

public class PluginEntryInstallationHandler : IEntryInstallationHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWorkshopService _workshopService;
    private readonly IPluginManagementService _pluginManagementService;

    public PluginEntryInstallationHandler(IHttpClientFactory httpClientFactory, IWorkshopService workshopService, IPluginManagementService pluginManagementService)
    {
        _httpClientFactory = httpClientFactory;
        _workshopService = workshopService;
        _pluginManagementService = pluginManagementService;
    }

    public async Task<EntryInstallResult> InstallAsync(IEntryDetails entry, IRelease release, Progress<StreamProgress> progress, CancellationToken cancellationToken)
    {
        // Ensure there is an installed entry
        InstalledEntry? installedEntry = _workshopService.GetInstalledEntry(entry.Id);
        if (installedEntry != null)
        {
            // If the folder already exists, we're not going to reinstall the plugin since files may be in use, consider our job done
            if (installedEntry.GetReleaseDirectory(release).Exists)
                return EntryInstallResult.FromSuccess(installedEntry);
        }
        else
        {
            // If none exists yet create one
            installedEntry = new InstalledEntry(entry, release);
            // Don't try to install a new plugin into an existing directory since files may be in use, consider our job screwed
            if (installedEntry.GetReleaseDirectory(release).Exists)
                return EntryInstallResult.FromFailure("Plugin is new but installation directory is not empty, try restarting Artemis");
        }

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

        // Create the release directory
        DirectoryInfo releaseDirectory = installedEntry.GetReleaseDirectory(release);
        releaseDirectory.Create();

        // Extract the archive
        stream.Seek(0, SeekOrigin.Begin);
        using ZipArchive archive = new(stream);
        archive.ExtractToDirectory(releaseDirectory.FullName);

        // If there is already a version of the plugin installed, disable it
        if (installedEntry.TryGetMetadata("PluginId", out Guid pluginId))
        {
            Plugin? currentVersion = _pluginManagementService.GetAllPlugins().FirstOrDefault(p => p.Guid == pluginId);
            if (currentVersion != null)
                _pluginManagementService.UnloadPlugin(currentVersion);
        }

        // Load the plugin, next time during startup this will happen automatically
        try
        {
            Plugin? plugin = _pluginManagementService.LoadPlugin(releaseDirectory);
            if (plugin == null)
                throw new ArtemisWorkshopException("Failed to load plugin, it may be incompatible");

            installedEntry.SetMetadata("PluginId", plugin.Guid);
        }
        catch (Exception e)
        {
            // If the plugin ended up being invalid yoink it out again, shoooo
            try
            {
                releaseDirectory.Delete(true);
            }
            catch (Exception)
            {
                // ignored, will get cleaned up as an orphaned file
            }

            _workshopService.RemoveInstalledEntry(installedEntry);
            return EntryInstallResult.FromFailure(e.Message);
        }

        _workshopService.SaveInstalledEntry(installedEntry);
        return EntryInstallResult.FromSuccess(installedEntry);
    }

    public Task<EntryUninstallResult> UninstallAsync(InstalledEntry installedEntry, CancellationToken cancellationToken)
    {
        // Disable the plugin
        if (installedEntry.TryGetMetadata("PluginId", out Guid pluginId))
        {
            Plugin? plugin = _pluginManagementService.GetAllPlugins().FirstOrDefault(p => p.Guid == pluginId);
            if (plugin != null)
                _pluginManagementService.UnloadPlugin(plugin);
        }

        // Attempt to remove from filesystem
        DirectoryInfo directory = installedEntry.GetDirectory();
        string? message = null;
        try
        {
            if (directory.Exists)
                directory.Delete(true);
        }
        catch (Exception)
        {
            message = "Failed to clean up files, you may need to restart Artemis";
        }

        // Remove entry
        _workshopService.RemoveInstalledEntry(installedEntry);
        return Task.FromResult(EntryUninstallResult.FromSuccess(message));
    }
}