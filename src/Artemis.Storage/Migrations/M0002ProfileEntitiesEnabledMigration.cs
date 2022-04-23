using System.Collections.Generic;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Migrations.Interfaces;
using LiteDB;

namespace Artemis.Storage.Migrations
{
    public class M0002ProfileEntitiesEnabledMigration : IStorageMigration
    {
        public int UserVersion => 2;

        public void Apply(LiteRepository repository)
        {
            List<ProfileEntity> profiles = repository.Query<ProfileEntity>().ToList();
            foreach (ProfileEntity profileEntity in profiles)
            {
                foreach (FolderEntity profileEntityFolder in profileEntity.Folders)
                {
                    profileEntityFolder.Suspended = false;
                    // Commented out during Avalonia port when Suspended was moved into the LayerEffect's LayerProperties
                    // foreach (LayerEffectEntity layerEffectEntity in profileEntityFolder.LayerEffects)
                    //     layerEffectEntity.Suspended = false;
                }

                foreach (LayerEntity profileEntityLayer in profileEntity.Layers)
                {
                    profileEntityLayer.Suspended = false;
                    // Commented out during Avalonia port when Suspended was moved into the LayerEffect's LayerProperties
                    // foreach (LayerEffectEntity layerEffectEntity in profileEntityLayer.LayerEffects)
                    //     layerEffectEntity.Suspended = false;
                }

                repository.Upsert(profileEntity);
            }
        }
    }
}