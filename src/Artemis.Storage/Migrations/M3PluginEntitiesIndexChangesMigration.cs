using Artemis.Storage.Migrations.Interfaces;
using LiteDB;

namespace Artemis.Storage.Migrations
{
    public class M3PluginEntitiesIndexChangesMigration : IStorageMigration
    {
        public int UserVersion => 3;

        public void Apply(LiteRepository repository)
        {
            if (repository.Database.CollectionExists("PluginEntity"))
                repository.Database.DropCollection("PluginEntity");
            if (repository.Database.CollectionExists("PluginSettingEntity"))
                repository.Database.DropCollection("PluginSettingEntity");
        }
    }
}