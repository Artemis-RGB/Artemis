using Artemis.Storage.Migrations.Interfaces;
using LiteDB;

namespace Artemis.Storage.Migrations
{
    public class M9DeviceCalibration : IStorageMigration
    {
        public int UserVersion => 9;

        /// <inheritdoc />
        public void Apply(LiteRepository repository)
        {
            ILiteCollection<BsonDocument> collection = repository.Database.GetCollection("SurfaceEntity");
            foreach (BsonDocument bsonDocument in collection.FindAll())
            {
                foreach (BsonValue bsonDevice in bsonDocument["DeviceEntities"].AsArray)
                {
                    bsonDevice["RedScale"] = 1;
                    bsonDevice["GreenScale"] = 1;
                    bsonDevice["BlueScale"] = 1;
                }

                collection.Update(bsonDocument);
            }
        }
    }
}