using Artemis.Storage.Migrations.Interfaces;
using LiteDB;

namespace Artemis.Storage.Migrations
{
    public class M6PredicateAbstraction : IStorageMigration
    {
        public int UserVersion => 7;

        public void Apply(LiteRepository repository)
        {
            ILiteCollection<BsonDocument> collection = repository.Database.GetCollection("ProfileEntity");
            foreach (BsonDocument bsonDocument in collection.FindAll())
            {
                
                foreach (BsonValue bsonLayer in bsonDocument["Layers"].AsArray)
                {
                    bsonLayer["DisplayCondition"] = null;
                    foreach (BsonValue bsonPropertyEntity in bsonLayer["PropertyEntities"].AsArray)
                        bsonPropertyEntity["DataBindingEntities"].AsArray.Clear();
                }

                foreach (BsonValue bsonLayer in bsonDocument["Folders"].AsArray)
                {
                    bsonLayer["DisplayCondition"] = null;
                    foreach (BsonValue bsonPropertyEntity in bsonLayer["PropertyEntities"].AsArray)
                        bsonPropertyEntity["DataBindingEntities"].AsArray.Clear();
                }

                collection.Update(bsonDocument);
            }
        }
    }
}