using Artemis.Storage.Migrations.Interfaces;
using LiteDB;

namespace Artemis.Storage.Migrations
{
    public class M0005DataBindingTypes : IStorageMigration
    {
        public int UserVersion => 5;

        public void Apply(LiteRepository repository)
        {
            ILiteCollection<BsonDocument> collection = repository.Database.GetCollection("ProfileEntity");
            foreach (BsonDocument bsonDocument in collection.FindAll())
            {
                foreach (BsonValue bsonLayer in bsonDocument["Layers"].AsArray)
                {
                    foreach (BsonValue bsonPropertyEntity in bsonLayer["PropertyEntities"].AsArray)
                        bsonPropertyEntity["DataBindingEntities"].AsArray.Clear();
                }

                foreach (BsonValue bsonLayer in bsonDocument["Folders"].AsArray)
                {
                    foreach (BsonValue bsonPropertyEntity in bsonLayer["PropertyEntities"].AsArray)
                        bsonPropertyEntity["DataBindingEntities"].AsArray.Clear();
                }

                collection.Update(bsonDocument);
            }
        }
    }
}