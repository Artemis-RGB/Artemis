using Artemis.Storage.Migrator.Legacy.Migrations;
using LiteDB;
using Serilog;

namespace Artemis.Storage.Migrator.Legacy;

public static class StorageMigrationService
{
    public static void ApplyPendingMigrations(ILogger logger, LiteRepository repository, IList<IStorageMigration> migrations)
    {
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