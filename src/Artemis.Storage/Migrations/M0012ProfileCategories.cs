using System.Linq;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Migrations.Interfaces;
using LiteDB;

namespace Artemis.Storage.Migrations
{
    public class M0012ProfileCategories : IStorageMigration
    {
        public int UserVersion => 12;

        public void Apply(LiteRepository repository)
        {
            ILiteCollection<ProfileCategoryEntity> profileCategories = repository.Database.GetCollection<ProfileCategoryEntity>();
            profileCategories.EnsureIndex(s => s.Name, true);
            ProfileCategoryEntity? profileCategoryEntity = profileCategories.Find(c => c.Name == "Converted").FirstOrDefault();
            if (profileCategoryEntity == null)
            {
                profileCategoryEntity = new ProfileCategoryEntity {Name = "Imported"};
                profileCategories.Insert(profileCategoryEntity);
            }

            ILiteCollection<BsonDocument> collection = repository.Database.GetCollection("ProfileEntity");
            foreach (BsonDocument bsonDocument in collection.FindAll())
            {
                // Profiles with a ModuleId have not been converted
                if (bsonDocument.ContainsKey("ModuleId"))
                {
                    string moduleId = bsonDocument["ModuleId"].AsString;
                    bsonDocument.Remove("ModuleId");

                    ProfileConfigurationEntity profileConfiguration = new()
                    {
                        Name = bsonDocument["Name"].AsString,
                        MaterialIcon = "ApplicationImport",
                        ModuleId = moduleId,
                        ProfileId = bsonDocument["_id"].AsGuid
                    };

                    profileCategoryEntity.ProfileConfigurations.Add(profileConfiguration);
                    collection.Update(bsonDocument);
                }
            }

            profileCategories.Update(profileCategoryEntity);
        }
    }
}