using System.Collections.Generic;
using System.Linq;
using Artemis.Storage.Migrations.Interfaces;
using LiteDB;

namespace Artemis.Storage.Migrations;

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