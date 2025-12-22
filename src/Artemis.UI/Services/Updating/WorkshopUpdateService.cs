using System;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Extensions;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared.Utilities;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Handlers.InstallationHandlers;
using Artemis.WebClient.Workshop.Models;
using Artemis.WebClient.Workshop.Services;
using Serilog;
using StrawberryShake;

namespace Artemis.UI.Services.Updating;

public class WorkshopUpdateService : IWorkshopUpdateService
{
    private readonly ILogger _logger;
    private readonly IWorkshopClient _client;
    private readonly IWorkshopService _workshopService;
    private readonly Lazy<IUpdateNotificationProvider> _updateNotificationProvider;
    private readonly PluginSetting<bool> _showNotifications;

    public WorkshopUpdateService(ILogger logger, IWorkshopClient client, IWorkshopService workshopService, ISettingsService settingsService,
        Lazy<IUpdateNotificationProvider> updateNotificationProvider)
    {
        _logger = logger;
        _client = client;
        _workshopService = workshopService;
        _updateNotificationProvider = updateNotificationProvider;
        _showNotifications = settingsService.GetSetting("Workshop.ShowNotifications", true);
    }

    public async Task AutoUpdateEntries()
    {
        _logger.Information("Checking for workshop updates");
        int checkedEntries = 0;
        int updatedEntries = 0;

        foreach (InstalledEntry entry in _workshopService.GetInstalledEntries())
        {
            if (!entry.AutoUpdate)
                continue;

            checkedEntries++;
            bool updated = await AutoUpdateEntry(entry);
            if (updated)
                updatedEntries++;
        }

        _logger.Information("Checked {CheckedEntries} entries, updated {UpdatedEntries}", checkedEntries, updatedEntries);

        if (updatedEntries > 0 && _showNotifications.Value)
            _updateNotificationProvider.Value.ShowWorkshopNotification(updatedEntries);
    }

    public async Task<bool> AutoUpdateEntry(InstalledEntry installedEntry)
    {
        try
        {
            // Query the latest version
            IOperationResult<IGetEntryLatestReleaseByIdResult> latestReleaseResult = await _client.GetEntryLatestReleaseById.ExecuteAsync(installedEntry.Id);
            IEntrySummary? entry = latestReleaseResult.Data?.Entry?.LatestRelease?.Entry;
            if (entry == null)
                return false;
            if (latestReleaseResult.Data?.Entry?.LatestRelease is not IRelease latestRelease)
                return false;
            if (latestRelease.Id == installedEntry.ReleaseId)
                return false;

            if (!latestRelease.IsCompatible())
            {
                _logger.Information("Skipping auto-update of entry {Entry} because it requires a newer version of Artemis ({RequiredVersion})", entry, latestRelease.MinimumVersion);
                return false;
            }

            _logger.Information("Auto-updating entry {Entry} to version {Version}", entry, latestRelease.Version);
            EntryInstallResult updateResult = await _workshopService.InstallEntry(entry, latestRelease, new Progress<StreamProgress>(), CancellationToken.None);

            // This happens during installation too but not on our reference of the entry
            if (updateResult.IsSuccess)
                installedEntry.ApplyRelease(latestRelease);

            if (updateResult.IsSuccess)
                _logger.Information("Auto-update successful for entry {Entry}", entry);
            else
                _logger.Warning("Auto-update failed for entry {Entry}: {Message}", entry, updateResult.Message);

            return updateResult.IsSuccess;
        }
        catch (Exception e)
        {
            _logger.Warning(e, "Auto-update failed for entry {Entry}", installedEntry);
        }

        return false;
    }

    /// <inheritdoc />
    public void DisableNotifications()
    {
        _showNotifications.Value = false;
        _showNotifications.Save();
    }
}