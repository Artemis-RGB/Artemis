using Artemis.Storage.Migrations.Interfaces;
using LiteDB;

namespace Artemis.Storage.Migrations
{
    public class M10BetterDataBindings : IStorageMigration
    {
        private void Migrate(BsonValue bsonValue)
        {
            if (!bsonValue.IsDocument || !bsonValue.AsDocument.TryGetValue("PropertyEntities", out BsonValue propertyEntities))
                return;

            foreach (BsonValue propertyEntity in propertyEntities.AsArray)
            {
                if (!propertyEntity.AsDocument.TryGetValue("DataBindingEntities", out BsonValue dataBindingEntities))
                    continue;
                foreach (BsonValue dataBindingEntity in dataBindingEntities.AsArray)
                {
                    if (!dataBindingEntity.AsDocument.TryGetValue("TargetExpression", out BsonValue targetExpression))
                        continue;
                    string value = targetExpression.AsString;
                    if (value == "value => value" || value == "b => b")
                    {
                        dataBindingEntity.AsDocument["Identifier"] = "Value";
                    }
                    else
                    {
                        string selector = value.Split("=>")[1];
                        string property = selector.Split(".")[1];
                        dataBindingEntity.AsDocument["Identifier"] = property;
                    }

                    dataBindingEntity.AsDocument.Remove("TargetExpression");
                }
            }
        }

        public int UserVersion => 10;

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