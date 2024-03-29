﻿using Artemis.Storage.Legacy.Entities.Profile.Conditions;

namespace Artemis.Storage.Legacy.Entities.Profile.Abstract;

internal abstract class RenderElementEntity
{
    public Guid Id { get; set; }
    public Guid ParentId { get; set; }

    public List<LayerEffectEntity> LayerEffects { get; set; } = new();

    public IConditionEntity? DisplayCondition { get; set; }
    public TimelineEntity? Timeline { get; set; }
}