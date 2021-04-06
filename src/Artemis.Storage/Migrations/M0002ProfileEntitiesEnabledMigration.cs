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
                    profileEntityFolder.Enabled = true;
                    foreach (LayerEffectEntity layerEffectEntity in profileEntityFolder.LayerEffects)
                        layerEffectEntity.Enabled = true;
                }

                foreach (LayerEntity profileEntityLayer in profileEntity.Layers)
                {
                    profileEntityLayer.Enabled = true;
                    foreach (LayerEffectEntity layerEffectEntity in profileEntityLayer.LayerEffects)
                        layerEffectEntity.Enabled = true;
                }

                repository.Upsert(profileEntity);
            }
        }
    }
}