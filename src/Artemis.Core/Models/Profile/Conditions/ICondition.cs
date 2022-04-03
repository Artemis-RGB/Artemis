using System;
using Artemis.Storage.Entities.Profile.Abstract;

namespace Artemis.Core;

/// <summary>
///     Represents a condition applied to a <see cref="ProfileElement" />
/// </summary>
public interface ICondition : IDisposable, IStorageModel
{
    /// <summary>
    ///     Gets the entity used to store this condition
    /// </summary>
    public IConditionEntity Entity { get; }

    /// <summary>
    ///     Gets the profile element this condition applies to
    /// </summary>
    public ProfileElement ProfileElement { get; }

    /// <summary>
    ///     Gets a boolean indicating whether the condition is currently met
    /// </summary>

    bool IsMet { get; }

    /// <summary>
    ///     Updates the condition
    /// </summary>
    void Update();

    /// <summary>
    ///     Applies the display condition to the provided timeline
    /// </summary>
    /// <param name="isMet"></param>
    /// <param name="wasMet"></param>
    /// <param name="timeline">The timeline to apply the display condition to</param>
    void ApplyToTimeline(bool isMet, bool wasMet, Timeline timeline);
}

/// <summary>
///     Represents a condition applied to a <see cref="ProfileElement" /> using a <see cref="INodeScript" />
/// </summary>
public interface INodeScriptCondition : ICondition
{
    /// <summary>
    ///     Gets the node script of this node script condition
    /// </summary>
    INodeScript? NodeScript { get; }

    /// <summary>
    ///     Loads the node script this node script condition uses
    /// </summary>
    void LoadNodeScript();
}