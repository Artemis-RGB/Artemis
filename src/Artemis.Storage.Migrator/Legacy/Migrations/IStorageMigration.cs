using LiteDB;

namespace Artemis.Storage.Migrator.Legacy.Migrations;

public interface IStorageMigration
{
    int UserVersion { get; }
    void Apply(LiteRepository repository);
}