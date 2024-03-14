using Artemis.Core;
using Artemis.Storage.Legacy.Entities.General;
using Artemis.Storage.Legacy.Entities.Plugins;
using Artemis.Storage.Legacy.Entities.Profile;
using Artemis.Storage.Legacy.Entities.Surface;
using Artemis.Storage.Legacy.Entities.Workshop;
using Artemis.Storage.Legacy.Migrations;
using Artemis.Storage.Legacy.Migrations.Storage;
using DryIoc;
using LiteDB;
using Serilog;

namespace Artemis.Storage.Legacy;

public static class LegacyMigrationService
{
    public static void MigrateToSqlite(IContainer container)
    {
        ILogger logger = container.Resolve<ILogger>();
        
        // Before creating a DB context which is kinda expensive, check if there's anything to migrate
        if (!File.Exists(Path.Combine(Constants.DataFolder, "database.db")))
        {
            logger.Information("No legacy database found, nothing to migrate");
            return;
        }

        // If the legacy database has already been migrated, but the old DB failed to be deleted, we don't want to migrate again
        // In a future update we'll clean up the old DB if it's still there, for now lets leave people's files alone
        if (File.Exists(Path.Combine(Constants.DataFolder, "legacy.db")))
        {
            logger.Information("Legacy database already migrated, nothing to do");
            return;
        }
        
        using ArtemisDbContext dbContext = container.Resolve<ArtemisDbContext>();
        MigrateToSqlite(logger, dbContext);
    }
    
    public static void MigrateToSqlite(ILogger logger, ArtemisDbContext dbContext)
    {
        if (!File.Exists(Path.Combine(Constants.DataFolder, "database.db")))
        {
            logger.Information("No legacy database found, nothing to migrate");
            return;
        }

        logger.Information("Migrating legacy database...");

        try
        {
            // Copy the database before using it, we're going to make some modifications to it and we don't want to mess up the original
            string databasePath = Path.Combine(Constants.DataFolder, "database.db");
            string tempPath = Path.Combine(Constants.DataFolder, "temp.db");
            File.Copy(databasePath, tempPath, true);

            using LiteRepository repository = new($"FileName={tempPath}");

            // Apply pending LiteDB migrations, this includes a migration that transforms namespaces to Artemis.Storage.Legacy
            ApplyPendingMigrations(logger, repository);

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

            // After a successful migration, keep the legacy database around for a while
            File.Move(Path.Combine(Constants.DataFolder, "database.db"), Path.Combine(Constants.DataFolder, "legacy.db"));

            logger.Information("Legacy database migrated");
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
    }

    private static void ApplyPendingMigrations(ILogger logger, LiteRepository repository)
    {
        List<IStorageMigration> migrations =
        [
            new M0020AvaloniaReset(),
            new M0021GradientNodes(),
            new M0022TransitionNodes(),
            new M0023LayoutProviders(),
            new M0024NodeProviders(),
            new M0025NodeProvidersProfileConfig(),
            new M0026NodeStorage(logger),
            new M0027Namespace()
        ];

        foreach (IStorageMigration storageMigration in migrations.OrderBy(m => m.UserVersion))
        {
            if (repository.Database.UserVersion >= storageMigration.UserVersion)
                continue;

            logger.Information("Applying storage migration {storageMigration} to update DB from v{oldVersion} to v{newVersion}",
                storageMigration.GetType().Name, repository.Database.UserVersion, storageMigration.UserVersion);

            repository.Database.BeginTrans();
            try
            {
                storageMigration.Apply(repository);
            }
            catch (Exception)
            {
                repository.Database.Rollback();
                throw;
            }

            repository.Database.Commit();
            repository.Database.UserVersion = storageMigration.UserVersion;
        }
    }
}