using System.Collections.Generic;
using System.Linq;
using Artemis.Storage.Migrations.Interfaces;
using LiteDB;
using Serilog;

namespace Artemis.Storage
{
    public class StorageMigrationService
    {
        private readonly ILogger _logger;
        private readonly LiteRepository _repository;
        private readonly List<IStorageMigration> _migrations;

        public StorageMigrationService(ILogger logger, LiteRepository repository, List<IStorageMigration> migrations)
        {
            _logger = logger;
            _repository = repository;
            _migrations = migrations;

            ApplyPendingMigrations();
        }

        public void ApplyPendingMigrations()
        {
            foreach (var storageMigration in _migrations.OrderBy(m => m.UserVersion))
            {
                if (_repository.Database.UserVersion >= storageMigration.UserVersion)
                    continue;

                _logger.Information("Applying storage migration {storageMigration} to update DB from v{oldVersion} to v{newVersion}",
                    storageMigration.GetType().Name, _repository.Database.UserVersion, storageMigration.UserVersion);
                storageMigration.Apply(_repository);
                _repository.Database.UserVersion = storageMigration.UserVersion;
            }
        }
    }
}