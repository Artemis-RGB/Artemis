using LiteDB;

namespace Artemis.Storage.Legacy.Migrations.Storage;

internal class M0023LayoutProviders : IStorageMigration
{
    public int UserVersion => 23;

    public void Apply(LiteRepository repository)
    {
        ILiteCollection<BsonDocument> deviceEntities = repository.Database.GetCollection("DeviceEntity");
        List<BsonDocument> toUpdate = new();

        foreach (BsonDocument bsonDocument in deviceEntities.FindAll())
        {
            if (bsonDocument.TryGetValue("CustomLayoutPath", out BsonValue customLayoutPath) && customLayoutPath.IsString && !string.IsNullOrEmpty(customLayoutPath.AsString))
            {
                bsonDocument.Add("LayoutType", new BsonValue("CustomPath"));
                bsonDocument.Add("LayoutParameter", new BsonValue(customLayoutPath.AsString));
            }
            else if (bsonDocument.TryGetValue("DisableDefaultLayout", out BsonValue disableDefaultLayout) && disableDefaultLayout.AsBoolean)
            {
                bsonDocument.Add("LayoutType", new BsonValue("None"));
            }
            else
            {
                bsonDocument.Add("LayoutType", new BsonValue("Default"));
            }

            bsonDocument.Remove("CustomLayoutPath");
            bsonDocument.Remove("DisableDefaultLayout");
            toUpdate.Add(bsonDocument);
        }

        deviceEntities.Update(toUpdate);
    }
}