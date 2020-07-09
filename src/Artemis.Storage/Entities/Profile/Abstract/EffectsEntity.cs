using System.Collections.Generic;

namespace Artemis.Storage.Entities.Profile.Abstract
{
    public abstract class EffectsEntity : PropertiesEntity
    {
        public List<LayerEffectEntity> LayerEffects { get; set; }
    }
}