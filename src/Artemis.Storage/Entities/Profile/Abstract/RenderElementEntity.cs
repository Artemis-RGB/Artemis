using System;
using System.Collections.Generic;
using Artemis.Storage.Entities.Profile.Conditions;
using Artemis.Storage.Entities.Profile.Nodes;

namespace Artemis.Storage.Entities.Profile.Abstract
{
    public abstract class RenderElementEntity
    {
        public Guid Id { get; set; }
        public Guid ParentId { get; set; }

        public List<LayerEffectEntity> LayerEffects { get; set; }
        public List<PropertyEntity> PropertyEntities { get; set; }
        public List<string> ExpandedPropertyGroups { get; set; }

        public DataModelConditionGroupEntity DisplayCondition { get; set; }
        public TimelineEntity Timeline { get; set; }

        public NodeScriptEntity NodeScript { get; set; }
    }
}