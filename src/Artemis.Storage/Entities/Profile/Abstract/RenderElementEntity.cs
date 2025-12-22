using System;
using System.Collections.Generic;

namespace Artemis.Storage.Entities.Profile.Abstract;

public abstract class RenderElementEntity
{
    public Guid Id { get; set; }
    public Guid ParentId { get; set; }

    public List<LayerEffectEntity> LayerEffects { get; set; } = [];

    public IConditionEntity? DisplayCondition { get; set; }
    public TimelineEntity? Timeline { get; set; }
}