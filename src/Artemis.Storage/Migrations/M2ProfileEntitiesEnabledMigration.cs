using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Migrations.Interfaces;
using LiteDB;

namespace Artemis.Storage.Migrations
{
    public class M2ProfileEntitiesEnabledMigration : IStorageMigration
    {
        public int UserVersion => 2;

        public void Apply(LiteRepository repository)
        {
            var profiles = repository.Query<ProfileEntity>().ToList();
            foreach (var profileEntity in profiles)
            {
                foreach (var profileEntityFolder in profileEntity.Folders)
                {
                    profileEntityFolder.Enabled = true;
                    foreach (var layerEffectEntity in profileEntityFolder.LayerEffects) 
                        layerEffectEntity.Enabled = true;
                }

                foreach (var profileEntityLayer in profileEntity.Layers)
                {
                    profileEntityLayer.Enabled = true;
                    foreach (var layerEffectEntity in profileEntityLayer.LayerEffects)
                        layerEffectEntity.Enabled = true;
                }

                repository.Upsert(profileEntity);
            }
        }
    }
}