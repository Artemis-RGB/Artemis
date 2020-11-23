using Artemis.Storage.Migrations.Interfaces;
using LiteDB;

namespace Artemis.Storage.Migrations
{
    public class M1AttributeBasedPropertiesMigration : IStorageMigration
    {
        public int UserVersion => 1;

        public void Apply(LiteRepository repository)
        {
            // DropCollection will open a transaction so commit the current one
            repository.Database.Commit();
            if (repository.Database.CollectionExists("ProfileEntity"))
                repository.Database.DropCollection("ProfileEntity");
        }
    }
}