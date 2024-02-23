using LiteDB;

namespace Artemis.Storage.Migrations;

public interface IStorageMigration
{
    int UserVersion { get; }
    void Apply(LiteRepository repository);
}