using System;
using System.Linq;
using Artemis.Core.Internal;
using Artemis.Storage.Entities.Profile.Abstract;
using Artemis.Storage.Entities.Profile.Conditions;

namespace Artemis.Core;

/// <summary>
///     Represents a condition that is based on a <see cref="DataModelEvent" />
/// </summary>
public class EventCondition : CorePropertyChanged, INodeScriptCondition
{
    private readonly string _displayName;
    private readonly EventConditionEntity _entity;
    private EventDefaultNode _eventNode;
    private DataModelPath? _eventPath;
    private DateTime _lastProcessedTrigger;
    private EventOverlapMode _overlapMode;
    private NodeScript<bool> _script;
    private EventTriggerMode _triggerMode;
    private bool _wasMet;

    /// <summary>
    ///     Creates a new instance of the <see cref="EventCondition" /> class
    /// </summary>
    public EventCondition(RenderProfileElement profileElement)
    {
        ProfileElement = profileElement;

        _entity = new EventConditionEntity();
        _displayName = profileElement.GetType().Name;
        _eventNode = new EventDefaultNode {X = -300};
        _script = new NodeScript<bool>($"Activate {_displayName}", $"Whether or not the event should activate the {_displayName}", ProfileElement.Profile);
    }

    internal EventCondition(EventConditionEntity entity, RenderProfileElement profileElement)
    {
        ProfileElement = profileElement;

        _entity = entity;
        _displayName = profileElement.GetType().Name;
        _eventNode = new EventDefaultNode();
        _script = null!;

        Load();
    }

    /// <summary>
    ///     Gets the script that drives the event condition
    /// </summary>
    public NodeScript<bool> Script
    {
        get => _script;
        set => SetAndNotify(ref _script, value);
    }

    /// <summary>
    ///     Gets or sets the path to the event that drives this event condition
    /// </summary>
    public DataModelPath? EventPath
    {
        get => _eventPath;
        set => SetAndNotify(ref _eventPath, value);
    }

    /// <summary>
    ///     Gets or sets how the condition behaves when the event fires.
    /// </summary>
    public EventTriggerMode TriggerMode
    {
        get => _triggerMode;
        set => SetAndNotify(ref _triggerMode, value);
    }

    /// <summary>
    ///     Gets or sets how the condition behaves when events trigger before the timeline finishes in the
    ///     <see cref="EventTriggerMode.Play" /> event trigger mode.
    /// </summary>
    public EventOverlapMode OverlapMode
    {
        get => _overlapMode;
        set => SetAndNotify(ref _overlapMode, value);
    }

    /// <summary>
    ///     Updates the event node, applying the selected event
    /// </summary>
    public void UpdateEventNode()
    {
        IDataModelEvent? dataModelEvent = EventPath?.GetValue() as IDataModelEvent;
        _eventNode.CreatePins(dataModelEvent);

        if (dataModelEvent != null && !Script.Nodes.Contains(_eventNode))
            Script.AddNode(_eventNode);
        else if (dataModelEvent == null && Script.Nodes.Contains(_eventNode))
            Script.RemoveNode(_eventNode);
    }

    /// <summary>
    ///     Gets the start node of the event script, if any
    /// </summary>
    /// <returns>The start node of the event script, if any.</returns>
    public INode GetStartNode()
    {
        return _eventNode;
    }

    private bool Evaluate()
    {
        if (EventPath?.GetValue() is not IDataModelEvent dataModelEvent || dataModelEvent.LastTrigger <= _lastProcessedTrigger)
            return false;

        _lastProcessedTrigger = dataModelEvent.LastTrigger;

        if (!Script.ExitNodeConnected)
            return true;

        Script.Run();
        return Script.Result;
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
        if (TriggerMode == EventTriggerMode.Toggle)
        {
            if (Evaluate())
                IsMet = !IsMet;
        }
        else
        {
            IsMet = Evaluate();
        }
    }

    /// <inheritdoc />
    public void UpdateTimeline(double deltaTime)
    {
        if (TriggerMode == EventTriggerMode.Toggle)
        {
            if (!IsMet && _wasMet)
                ProfileElement.Timeline.JumpToEnd();
            else if (IsMet && !_wasMet)
                ProfileElement.Timeline.JumpToStart();
        }
        else
        {
            if (IsMet && ProfileElement.Timeline.IsFinished)
            {
                ProfileElement.Timeline.JumpToStart();
            }
            else if (IsMet)
            {
                if (OverlapMode == EventOverlapMode.Restart)
                    ProfileElement.Timeline.JumpToStart();
                else if (OverlapMode == EventOverlapMode.Copy && ProfileElement is Layer layer)
                    layer.CreateCopyAsChild();
            }
        }

        ProfileElement.Timeline.Update(TimeSpan.FromSeconds(deltaTime), TriggerMode == EventTriggerMode.Toggle);
    }

    /// <inheritdoc />
    public void OverrideTimeline(TimeSpan position)
    {
        ProfileElement.Timeline.Override(position, TriggerMode == EventTriggerMode.Toggle && position > ProfileElement.Timeline.Length);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Script?.Dispose();
        EventPath?.Dispose();
    }

    #region Storage

    /// <inheritdoc />
    public void Load()
    {
        TriggerMode = (EventTriggerMode) _entity.TriggerMode;
        OverlapMode = (EventOverlapMode) _entity.OverlapMode;

        if (_entity.EventPath != null)
            EventPath = new DataModelPath(_entity.EventPath);

        Script = _entity.Script != null
            ? new NodeScript<bool>($"Activate {_displayName}", $"Whether or not the event should activate the {_displayName}", _entity.Script, ProfileElement.Profile)
            : new NodeScript<bool>($"Activate {_displayName}", $"Whether or not the event should activate the {_displayName}", ProfileElement.Profile);
        UpdateEventNode();
    }

    /// <inheritdoc />
    public void Save()
    {
        _entity.TriggerMode = (int) TriggerMode;
        _entity.OverlapMode = (int) OverlapMode;
        Script.Save();
        _entity.Script = Script?.Entity;
        EventPath?.Save();
        _entity.EventPath = EventPath?.Entity;
    }

    /// <inheritdoc />
    public INodeScript NodeScript => Script;

    /// <inheritdoc />
    public void LoadNodeScript()
    {
        Script.Load();

        // The load action may have created an event node, use that one over the one we have here
        INode? existingEventNode = Script.Nodes.FirstOrDefault(n => n.Id == EventDefaultNode.NodeId);
        if (existingEventNode != null)
            _eventNode = (EventDefaultNode) existingEventNode;
        
        UpdateEventNode();
        Script.LoadConnections();
        
    }

    #endregion
}

/// <summary>
///     Represents a mode for render elements to start their timeline when display conditions events are fired.
/// </summary>
public enum EventTriggerMode
{
    /// <summary>
    ///     Play the timeline once.
    /// </summary>
    Play,

    /// <summary>
    ///     Toggle repeating the timeline.
    /// </summary>
    Toggle
}

/// <summary>
///     Represents a mode for render elements to configure the behaviour of events that overlap i.e. trigger again before
///     the timeline finishes.
/// </summary>
public enum EventOverlapMode
{
    /// <summary>
    ///     Stop the current run and restart the timeline
    /// </summary>
    Restart,

    /// <summary>
    ///     Play another copy of the timeline on top of the current run
    /// </summary>
    Copy,

    /// <summary>
    ///     Ignore subsequent event fires until the timeline finishes
    /// </summary>
    Ignore
}