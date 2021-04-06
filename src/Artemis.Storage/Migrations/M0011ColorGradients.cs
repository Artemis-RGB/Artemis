using Artemis.Storage.Migrations.Interfaces;
using LiteDB;

namespace Artemis.Storage.Migrations
{
    public class M0011ColorGradients : IStorageMigration
    {
        private void Migrate(BsonValue bsonValue)
        {
            if (!bsonValue.IsDocument || !bsonValue.AsDocument.TryGetValue("PropertyEntities", out BsonValue propertyEntities))
                return;

            foreach (BsonValue propertyEntity in propertyEntities.AsArray)
            {
                if (propertyEntity["Value"] == null)
                    continue;
                string valueString = propertyEntity["Value"].AsString;
                if (valueString == null)
                    continue;
                if (!valueString.StartsWith("{\"Stops\":[{") || !valueString.EndsWith("}]}"))
                    continue;

                valueString = valueString.Replace("{\"Stops\":[{", "[{");
                valueString = valueString.Replace("}]}", "}]");
                propertyEntity["Value"] = valueString;
            }
        }

        public int UserVersion => 11;

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
    }
}