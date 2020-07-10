using System.Collections.Generic;

namespace Artemis.Storage.Entities.Profile.Abstract
{
    public abstract class RenderElementEntity
    {
        public List<LayerEffectEntity> LayerEffects { get; set; }
        public List<PropertyEntity> PropertyEntities { get; set; }
        public List<string> ExpandedPropertyGroups { get; set; }

        public DisplayConditionGroupEntity RootDisplayCondition { get; set; }
    }
}