using Artemis.Core;
using Artemis.Storage.Entities.Plugins;
using Artemis.Storage.Repositories.Interfaces;
using Artemis.UI.Shared.Utilities;
using Artemis.WebClient.Workshop.Services;
using Serilog;
using StrawberryShake;

namespace Artemis.WebClient.Workshop;

public static class BuiltInPluginsMigrator
{
    private static readonly Guid[] ObsoleteBuiltInPlugins =
    [
        new("4e1e54fd-6636-40ad-afdc-b3b0135feab2"),
        new("cad475d3-c621-4ec7-bbfc-784e3b4723ce"),
        new("ab41d601-35e0-4a73-bf0b-94509b006ab0"),
        new("27d124e3-48e8-4b0a-8a5e-d5e337a88d4a")
    ];

    public static async Task<bool> Migrate(IWorkshopService workshopService, IWorkshopClient workshopClient, ILogger logger, IPluginRepository pluginRepository)
    {
        // If no default plugins are present (later installs), do nothing
        DirectoryInfo pluginDirectory = new(Constants.PluginsFolder);
        if (!pluginDirectory.Exists)
        {
            return true;
        }

        // Load plugin info, the plugin management service isn't available yet (which is exactly what we want)
        List<(PluginInfo PluginInfo, DirectoryInfo Directory)> plugins = [];
        foreach (DirectoryInfo subDirectory in pluginDirectory.EnumerateDirectories())
        {
            try
            {
                // Load the metadata
                string metadataFile = Path.Combine(subDirectory.FullName, "plugin.json");
                if (File.Exists(metadataFile))
                    plugins.Add((CoreJson.Deserialize<PluginInfo>(await File.ReadAllTextAsync(metadataFile))!, subDirectory));
            }
            catch (Exception)
            {
                // ignored, who knows what old stuff people might have in their plugins folder
            }
        }

        if (plugins.Count == 0)
        {
            return true;
        }

        IWorkshopService.WorkshopStatus workshopStatus = await workshopService.GetWorkshopStatus(CancellationToken.None);
        if (!workshopStatus.IsReachable)
        {
            logger.Warning("MigrateBuiltInPlugins - Cannot migrate built-in plugins because the workshop is unreachable");
            return false;
        }

        logger.Information("MigrateBuiltInPlugins - Migrating built-in plugins to workshop entries");
        IOperationResult<IGetDefaultPluginsResult> result = await workshopClient.GetDefaultPlugins.ExecuteAsync(100, null, CancellationToken.None);
        List<IGetDefaultPlugins_EntriesV2_Edges_Node> entries = result.Data?.EntriesV2?.Edges?.Select(e => e.Node).ToList() ?? [];
        while (result.Data?.EntriesV2?.PageInfo is {HasNextPage: true})
        {
            result = await workshopClient.GetDefaultPlugins.ExecuteAsync(100, result.Data.EntriesV2.PageInfo.EndCursor, CancellationToken.None);
            if (result.Data?.EntriesV2?.Edges != null)
                entries.AddRange(result.Data.EntriesV2.Edges.Select(e => e.Node));
        }

        logger.Information("MigrateBuiltInPlugins - Found {Count} default plugins in the workshop", entries.Count);
        foreach (IGetDefaultPlugins_EntriesV2_Edges_Node entry in entries)
        {
            // Skip entries without plugin info or releases, shouldn't happen but theoretically possible
            if (entry.PluginInfo == null || entry.LatestRelease == null)
                continue;

            // Find a built-in plugin
            (PluginInfo? pluginInfo, DirectoryInfo? directory) = plugins.FirstOrDefault(p => p.PluginInfo.Guid == entry.PluginInfo.PluginGuid);
            if (pluginInfo == null || directory == null)
                continue;

            // If the plugin is enabled, install the workshop equivalent (the built-in plugin will be removed by the install process)
            PluginEntity? entity = pluginRepository.GetPluginByPluginGuid(pluginInfo.Guid);
            if (entity != null && entity.IsEnabled)
            {
                logger.Information("MigrateBuiltInPlugins - Migrating built-in plugin {Plugin} to workshop entry {Entry}", pluginInfo.Name, entry);
                await workshopService.InstallEntry(entry, entry.LatestRelease, new Progress<StreamProgress>(), CancellationToken.None);
            }

            // Remove the built-in plugin, it's no longer needed
            directory.Delete(true);
        }

        // Remove obsolete built-in plugins
        foreach (Guid obsoleteBuiltInPlugin in ObsoleteBuiltInPlugins)
        {
            (PluginInfo? pluginInfo, DirectoryInfo? directory) = plugins.FirstOrDefault(p => p.PluginInfo.Guid == obsoleteBuiltInPlugin);
            if (pluginInfo == null || directory == null)
                continue;

            directory.Delete(true);
        }

        logger.Information("MigrateBuiltInPlugins - Finished migrating built-in plugins to workshop entries");
        return true;
    }
}