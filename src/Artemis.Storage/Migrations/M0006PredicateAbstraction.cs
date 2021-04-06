using System;
using Artemis.Storage.Migrations.Interfaces;
using LiteDB;

namespace Artemis.Storage.Migrations
{
    public class M0006PredicateAbstraction : IStorageMigration
    {
        public int UserVersion => 6;

        public void Apply(LiteRepository repository)
        {
            ILiteCollection<BsonDocument> collection = repository.Database.GetCollection("ProfileEntity");
            foreach (BsonDocument bsonDocument in collection.FindAll())
            {
                foreach (BsonValue bsonLayer in bsonDocument["Layers"].AsArray) 
                    Migrate(bsonLayer);

                foreach (BsonValue bsonLayer in bsonDocument["Folders"].AsArray)
                    Migrate(bsonLayer);

                collection.Update(bsonDocument);
            }
        }

        private void Migrate(BsonValue bsonValue)
        {
            if (bsonValue.IsArray)
            {
                foreach (BsonValue child in bsonValue.AsArray)
                    Migrate(child);
                return;
            }

            if (bsonValue.IsDocument)
            {
                // See if the document has a type
                if (bsonValue.AsDocument.TryGetValue("_type", out BsonValue typeValue))
                {
                    if (typeValue.AsString == "Artemis.Storage.Entities.Profile.Conditions.DataModelConditionPredicateEntity, Artemis.Storage")
                        bsonValue.AsDocument["_type"] = "Artemis.Storage.Entities.Profile.Conditions.DataModelConditionGeneralPredicateEntity, Artemis.Storage";
                }

                foreach (BsonValue documentValue in bsonValue.AsDocument.Values)
                    Migrate(documentValue);
            }
        }
    }
}