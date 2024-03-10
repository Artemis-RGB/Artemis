using LiteDB;

namespace Artemis.Storage.Legacy.Migrations;

public interface IStorageMigration
{
    int UserVersion { get; }
    void Apply(LiteRepository repository);
}