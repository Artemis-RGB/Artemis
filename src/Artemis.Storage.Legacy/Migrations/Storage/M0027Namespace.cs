using LiteDB;

namespace Artemis.Storage.Legacy.Migrations.Storage;

internal class M0027Namespace : IStorageMigration
{
    public int UserVersion => 27;

    public void Apply(LiteRepository repository)
    {
        ILiteCollection<BsonDocument> collection = repository.Database.GetCollection("ProfileEntity");
        List<BsonDocument> profilesToUpdate = [];

        foreach (BsonDocument profileBson in collection.FindAll())
        {
            MigrateDocument(profileBson);
            profilesToUpdate.Add(profileBson);
        }

        collection.Update(profilesToUpdate);
    }

    private void MigrateDocument(BsonDocument document)
    {
        foreach ((string? key, BsonValue? value) in document)
        {
            if (key == "_type")
                document[key] = document[key].AsString
                    .Replace("Artemis.Storage.Entities", "Artemis.Storage.Legacy.Entities")
                    .Replace(", Artemis.Storage", ", Artemis.Storage.Legacy");
            else if (value.IsDocument)
                MigrateDocument(value.AsDocument);
            else if (value.IsArray)
                foreach (BsonValue bsonValue in value.AsArray)
                {
                    if (bsonValue.IsDocument)
                        MigrateDocument(bsonValue.AsDocument);
                }
        }
    }
}