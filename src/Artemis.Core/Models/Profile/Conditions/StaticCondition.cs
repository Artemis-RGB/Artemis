using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Storage.Entities.Profile.Abstract;
using Artemis.Storage.Entities.Profile.Conditions;

namespace Artemis.Core;

/// <summary>
///     Represents a condition that is based on a data model value
/// </summary>
public class StaticCondition : CorePropertyChanged, INodeScriptCondition
{
    private readonly string _displayName;
    private readonly StaticConditionEntity _entity;
    private StaticPlayMode _playMode;
    private StaticStopMode _stopMode;
    private bool _wasMet;

    /// <summary>
    ///     Creates a new instance of the <see cref="StaticCondition" /> class
    /// </summary>
    public StaticCondition(RenderProfileElement profileElement)
    {
        _entity = new StaticConditionEntity();
        _displayName = profileElement.GetType().Name;

        ProfileElement = profileElement;
        Script = new NodeScript<bool>($"Activate {_displayName}", $"Whether or not this {_displayName} should be active", profileElement.Profile);
    }

    internal StaticCondition(StaticConditionEntity entity, RenderProfileElement profileElement)
    {
        _entity = entity;
        _displayName = profileElement.GetType().Name;

        ProfileElement = profileElement;
        Script = null!;

        Load();
    }

    /// <summary>
    ///     Gets the script that drives the static condition
    /// </summary>
    public NodeScript<bool> Script { get; private set; }

    /// <summary>
    ///     Gets or sets the mode in which the render element starts its timeline when display conditions are met
    /// </summary>
    public StaticPlayMode PlayMode
    {
        get => _playMode;
        set => SetAndNotify(ref _playMode, value);
    }

    /// <summary>
    ///     Gets or sets the mode in which the render element stops its timeline when display conditions are no longer met
    /// </summary>
    public StaticStopMode StopMode
    {
        get => _stopMode;
        set => SetAndNotify(ref _stopMode, value);
    }

    /// <inheritdoc />
    public IConditionEntity Entity => _entity;

    /// <inheritdoc />
    public RenderProfileElement ProfileElement { get; }

    /// <inheritdoc />
    public bool IsMet { get; private set; }

    /// <inheritdoc />
    public void Update()
    {
        _wasMet = IsMet;

        // No need to run the script if the parent isn't met anyway
        bool parentConditionMet = ProfileElement.Parent is not RenderProfileElement renderProfileElement || renderProfileElement.DisplayConditionMet;
        if (!parentConditionMet)
        {
            IsMet = false;
            return;
        }

        if (!Script.ExitNodeConnected)
        {
            IsMet = true;
        }
        else
        {
            Script.Run();
            IsMet = Script.Result;
        }
    }

    /// <inheritdoc />
    public void UpdateTimeline(double deltaTime)
    {
        if (IsMet && !_wasMet && ProfileElement.Timeline.IsFinished)
            ProfileElement.Timeline.JumpToStart();
        else if (!IsMet && _wasMet && StopMode == StaticStopMode.SkipToEnd)
            ProfileElement.Timeline.JumpToEndSegment();

        ProfileElement.Timeline.Update(TimeSpan.FromSeconds(deltaTime), PlayMode == StaticPlayMode.Repeat && IsMet);
    }

    /// <inheritdoc />
    public void OverrideTimeline(TimeSpan position)
    {
        ProfileElement.Timeline.Override(position, PlayMode == StaticPlayMode.Repeat && position > ProfileElement.Timeline.Length);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Script.Dispose();
    }

    #region Storage

    /// <inheritdoc />
    public void Load()
    {
        PlayMode = (StaticPlayMode) _entity.PlayMode;
        StopMode = (StaticStopMode) _entity.StopMode;

        Script = _entity.Script != null
            ? new NodeScript<bool>($"Activate {_displayName}", $"Whether or not this {_displayName} should be active", _entity.Script, ProfileElement.Profile)
            : new NodeScript<bool>($"Activate {_displayName}", $"Whether or not this {_displayName} should be active", ProfileElement.Profile);
    }

    /// <inheritdoc />
    public void Save()
    {
        _entity.PlayMode = (int) PlayMode;
        _entity.StopMode = (int) StopMode;

        // If the exit node isn't connected and there is only the exit node, don't save the script
        if (!Script.ExitNodeConnected && Script.Nodes.Count() == 1)
        {
            _entity.Script = null;
        }
        else
        {
            Script.Save();
            _entity.Script = Script.Entity;
        }
    }

    /// <inheritdoc />
    public INodeScript? NodeScript => Script;

    /// <inheritdoc />
    public void LoadNodeScript()
    {
        Script.Load();
    }

    #endregion

    #region Implementation of IPluginFeatureDependent

    /// <inheritdoc />
    public IEnumerable<PluginFeature> GetFeatureDependencies()
    {
        return Script.GetFeatureDependencies();
    }

    #endregion
}

/// <summary>
///     Represents a mode for render elements to start their timeline when display conditions are met
/// </summary>
public enum StaticPlayMode
{
    /// <summary>
    ///     Continue repeating the main segment of the timeline while the condition is met
    /// </summary>
    Repeat,

    /// <summary>
    ///     Only play the timeline once when the condition is met
    /// </summary>
    Once
}

/// <summary>
///     Represents a mode for render elements to stop their timeline when display conditions are no longer met
/// </summary>
public enum StaticStopMode
{
    /// <summary>
    ///     When conditions are no longer met, finish the the current run of the main timeline
    /// </summary>
    Finish,

    /// <summary>
    ///     When conditions are no longer met, skip to the end segment of the timeline
    /// </summary>
    SkipToEnd
}