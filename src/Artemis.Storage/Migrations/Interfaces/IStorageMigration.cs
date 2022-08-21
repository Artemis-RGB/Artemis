using LiteDB;

namespace Artemis.Storage.Migrations.Interfaces;

public interface IStorageMigration
{
    int UserVersion { get; }
    void Apply(LiteRepository repository);
}