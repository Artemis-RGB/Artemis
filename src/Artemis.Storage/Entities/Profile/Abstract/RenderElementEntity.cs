using System;
using System.Collections.Generic;
using Artemis.Storage.Entities.Profile.Conditions;

namespace Artemis.Storage.Entities.Profile.Abstract
{
    public abstract class RenderElementEntity
    {
        public TimeSpan StartSegmentLength { get; set; }
        public TimeSpan MainSegmentLength { get; set; }
        public TimeSpan EndSegmentLength { get; set; }
        public bool DisplayContinuously { get; set; }
        public bool AlwaysFinishTimeline { get; set; }

        public List<LayerEffectEntity> LayerEffects { get; set; }
        public List<PropertyEntity> PropertyEntities { get; set; }
        public List<string> ExpandedPropertyGroups { get; set; }

        public DisplayConditionGroupEntity RootDisplayCondition { get; set; }
    }
}