using System.Collections.Generic;

namespace Artemis.Storage.Entities.Profile
{
    public abstract class EffectsEntity : PropertiesEntity
    {
        public List<LayerEffectEntity> LayerEffects { get; set; }
    }
}