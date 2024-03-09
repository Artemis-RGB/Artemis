using Artemis.Core;
using Artemis.Core.DryIoc;
using Artemis.Storage.Migrator.Legacy;
using Artemis.Storage.Migrator.Legacy.Entities.General;
using Artemis.Storage.Migrator.Legacy.Entities.Plugins;
using Artemis.Storage.Migrator.Legacy.Entities.Profile;
using Artemis.Storage.Migrator.Legacy.Entities.Surface;
using Artemis.Storage.Migrator.Legacy.Entities.Workshop;
using Artemis.Storage.Migrator.Legacy.Migrations.Storage;
using DryIoc;
using LiteDB;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Artemis.Storage.Migrator;

class Program
{
    static void Main(string[] args)
    {
        using Container container = new(rules => rules
            .WithMicrosoftDependencyInjectionRules()
            .WithConcreteTypeDynamicRegistrations()
            .WithoutThrowOnRegisteringDisposableTransient());

        container.RegisterCore();

        ILogger logger = container.Resolve<ILogger>();
        ArtemisDbContext dbContext = container.Resolve<ArtemisDbContext>();
        logger.Information("Applying pending migrations...");
        dbContext.Database.Migrate();
        logger.Information("Pending migrations applied");

        if (!File.Exists(Path.Combine(Constants.DataFolder, "database.db")))
        {
            logger.Information("No legacy database found, nothing to migrate");
            return;
        }

        logger.Information("Migrating legacy database...");

        try
        {
            MigrateLegacyDatabase(logger, dbContext);
            // After a successful migration, keep the legacy database around for a while
            File.Move(Path.Combine(Constants.DataFolder, "database.db"), Path.Combine(Constants.DataFolder, "legacy.db"));
        }
        catch (Exception e)
        {
            logger.Error(e, "Failed to migrate legacy database");
            throw;
        }
        finally
        {
            File.Delete(Path.Combine(Constants.DataFolder, "temp.db"));
        }

        logger.Information("Legacy database migrated");
    }

    private static void MigrateLegacyDatabase(ILogger logger, ArtemisDbContext dbContext)
    {
        // Copy the database before using it, we're going to make some modifications to it and we don't want to mess up the original
        string databasePath = Path.Combine(Constants.DataFolder, "database.db");
        string tempPath = Path.Combine(Constants.DataFolder, "temp.db");
        File.Copy(databasePath, tempPath, true);

        using LiteRepository repository = new($"FileName={tempPath}");

        // Apply pending LiteDB migrations, this includes a migration that transforms namespaces to Artemis.Storage.Migrator
        StorageMigrationService.ApplyPendingMigrations(
            logger,
            repository,
            [
                new M0020AvaloniaReset(),
                new M0021GradientNodes(),
                new M0022TransitionNodes(),
                new M0023LayoutProviders(),
                new M0024NodeProviders(),
                new M0025NodeProvidersProfileConfig(),
                new M0026NodeStorage(logger),
                new M0027Namespace(),
            ]
        );

        // Devices
        if (!dbContext.Devices.Any())
        {
            logger.Information("Migrating devices");
            List<DeviceEntity> legacyDevices = repository.Query<DeviceEntity>().Include(s => s.InputIdentifiers).ToList();
            dbContext.Devices.AddRange(legacyDevices.Select(l => l.Migrate()));
            dbContext.SaveChanges();
        }

        // Entries
        if (!dbContext.Entries.Any())
        {
            logger.Information("Migrating entries");
            List<EntryEntity> legacyEntries = repository.Query<EntryEntity>().ToList();
            dbContext.Entries.AddRange(legacyEntries.Select(l => l.Migrate()));
            dbContext.SaveChanges();
        }

        // Plugins
        if (!dbContext.Plugins.Any())
        {
            logger.Information("Migrating plugins");
            List<PluginEntity> legacyPlugins = repository.Query<PluginEntity>().ToList();
            dbContext.Plugins.AddRange(legacyPlugins.Select(l => l.Migrate()));
            dbContext.SaveChanges();
        }

        // PluginSettings
        if (!dbContext.PluginSettings.Any())
        {
            logger.Information("Migrating plugin settings");
            List<PluginSettingEntity> legacyPluginSettings = repository.Query<PluginSettingEntity>().ToList();
            dbContext.PluginSettings.AddRange(legacyPluginSettings.Select(l => l.Migrate()));
            dbContext.SaveChanges();
        }

        // ProfileCategories
        if (!dbContext.ProfileCategories.Any())
        {
            logger.Information("Migrating profile categories");
            List<ProfileCategoryEntity> legacyProfileCategories = repository.Query<ProfileCategoryEntity>().ToList();
            ILiteStorage<Guid> profileIcons = repository.Database.GetStorage<Guid>("profileIcons");
            List<ProfileEntity> legacyProfiles = repository.Query<ProfileEntity>().ToList();
            dbContext.ProfileCategories.AddRange(legacyProfileCategories.Select(l => l.Migrate(logger, legacyProfiles, profileIcons)));
            dbContext.SaveChanges();
        }

        // Releases
        if (!dbContext.Releases.Any())
        {
            logger.Information("Migrating releases");
            List<ReleaseEntity> legacyReleases = repository.Query<ReleaseEntity>().ToList();
            dbContext.Releases.AddRange(legacyReleases.Select(l => l.Migrate()));
            dbContext.SaveChanges();
        }
    }
}