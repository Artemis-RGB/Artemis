using Artemis.Storage.Migrations.Interfaces;
using LiteDB;

namespace Artemis.Storage.Migrations
{
    public class M0003PluginEntitiesIndexChangesMigration : IStorageMigration
    {
        public int UserVersion => 3;
        
        public void Apply(LiteRepository repository)
        {
            // DropCollection will open a transaction so commit the current one
            repository.Database.Commit();
            if (repository.Database.CollectionExists("PluginEntity"))
                repository.Database.DropCollection("PluginEntity");
            if (repository.Database.CollectionExists("PluginSettingEntity"))
                repository.Database.DropCollection("PluginSettingEntity");
        }
    }
}