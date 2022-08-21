using System;
using Artemis.Storage.Entities.Profile.Abstract;
using Artemis.Storage.Entities.Profile.Conditions;

namespace Artemis.Core;

/// <summary>
///     Represents a condition that is always true.
/// </summary>
public class AlwaysOnCondition : ICondition
{
    /// <summary>
    ///     Creates a new instance of the <see cref="AlwaysOnCondition" /> class.
    /// </summary>
    /// <param name="profileElement">The profile element this condition applies to.</param>
    public AlwaysOnCondition(RenderProfileElement profileElement)
    {
        ProfileElement = profileElement;
        Entity = new AlwaysOnConditionEntity();
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="AlwaysOnCondition" /> class.
    /// </summary>
    /// <param name="alwaysOnConditionEntity">The entity used to store this condition.</param>
    /// <param name="profileElement">The profile element this condition applies to.</param>
    public AlwaysOnCondition(AlwaysOnConditionEntity alwaysOnConditionEntity, RenderProfileElement profileElement)
    {
        ProfileElement = profileElement;
        Entity = alwaysOnConditionEntity;
    }

    /// <inheritdoc />
    public void Dispose()
    {
    }

    #region Implementation of IStorageModel

    /// <inheritdoc />
    public void Load()
    {
    }

    /// <inheritdoc />
    public void Save()
    {
    }

    #endregion

    #region Implementation of ICondition

    /// <inheritdoc />
    public IConditionEntity Entity { get; }

    /// <inheritdoc />
    public RenderProfileElement ProfileElement { get; }

    /// <inheritdoc />
    public bool IsMet { get; private set; }

    /// <inheritdoc />
    public void Update()
    {
        if (ProfileElement.Parent is RenderProfileElement parent)
            IsMet = parent.DisplayConditionMet;
        else
            IsMet = true;
    }

    /// <inheritdoc />
    public void UpdateTimeline(double deltaTime)
    {
        ProfileElement.Timeline.Update(TimeSpan.FromSeconds(deltaTime), true);
    }

    /// <inheritdoc />
    public void OverrideTimeline(TimeSpan position)
    {
        ProfileElement.Timeline.Override(position, position > ProfileElement.Timeline.Length);
    }

    #endregion
}