using LiteDB;

namespace Artemis.Storage.Migrator.Legacy.Migrations.Storage;

public class M0020AvaloniaReset : IStorageMigration
{
    public int UserVersion => 20;

    public void Apply(LiteRepository repository)
    {
        repository.Database.Commit();

        List<string> collectionNames = repository.Database.GetCollectionNames().ToList();
        foreach (string collectionName in collectionNames)
            repository.Database.DropCollection(collectionName);
    }
}