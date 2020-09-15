using Artemis.Storage.Migrations.Interfaces;
using LiteDB;

namespace Artemis.Storage.Migrations
{
    public class M5DataBindingTypes : IStorageMigration
    {
        public int UserVersion => 5;

        public void Apply(LiteRepository repository)
        {
            var collection = repository.Database.GetCollection("ProfileEntity");
            foreach (var bsonDocument in collection.FindAll())
            {
                foreach (var bsonLayer in bsonDocument["Layers"].AsArray)
                {
                    foreach (var bsonPropertyEntity in bsonLayer["PropertyEntities"].AsArray)
                        bsonPropertyEntity["DataBindingEntities"].AsArray.Clear();
                }

                foreach (var bsonLayer in bsonDocument["Folders"].AsArray)
                {
                    foreach (var bsonPropertyEntity in bsonLayer["PropertyEntities"].AsArray)
                        bsonPropertyEntity["DataBindingEntities"].AsArray.Clear();
                }

                collection.Update(bsonDocument);
            }
        }
    }
}