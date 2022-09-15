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
    private DataModelPath? _eventPath;
    private DateTime _lastProcessedTrigger;
    private object? _lastProcessedValue;
    private EventOverlapMode _overlapMode;
    private NodeScript<bool> _script;
    private IEventConditionNode _startNode;
    private EventToggleOffMode _toggleOffMode;
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
        _startNode = new EventConditionEventStartNode {X = -300};
        _script = new NodeScript<bool>($"Activate {_displayName}", $"Whether or not the event should activate the {_displayName}", ProfileElement.Profile);
    }

    internal EventCondition(EventConditionEntity entity, RenderProfileElement profileElement)
    {
        ProfileElement = profileElement;

        _entity = entity;
        _displayName = profileElement.GetType().Name;
        _startNode = new EventConditionEventStartNode();
        _script = null!;

        Load();
    }

    /// <summary>
    ///     Gets the script that drives the event condition
    /// </summary>
    public NodeScript<bool> Script
    {
        get => _script;
        private set => SetAndNotify(ref _script, value);
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
    ///     Gets or sets the mode for render elements when toggling off the event when using
    ///     <see cref="EventTriggerMode.Toggle" />.
    /// </summary>
    public EventToggleOffMode ToggleOffMode
    {
        get => _toggleOffMode;
        set => SetAndNotify(ref _toggleOffMode, value);
    }

    /// <summary>
    ///     Updates the event node, applying the selected event
    /// </summary>
    public void UpdateEventNode()
    {
        if (EventPath == null)
            return;

        Type? pathType = EventPath.GetPropertyType();
        if (pathType == null)
            return;

        // Create an event node if the path type is a data model event
        if (pathType.IsAssignableTo(typeof(IDataModelEvent)))
        {
            EventConditionEventStartNode eventNode;
            // Ensure the start node is an event node
            if (_startNode is not EventConditionEventStartNode node)
            {
                eventNode = new EventConditionEventStartNode();
                ReplaceStartNode(eventNode);
                _startNode = eventNode;
            }
            else
            {
                eventNode = node;
            }

            IDataModelEvent? dataModelEvent = EventPath?.GetValue() as IDataModelEvent;
            eventNode.CreatePins(dataModelEvent);
        }
        // Create a value changed node if the path type is a regular value
        else
        {
            // Ensure the start nod is a value changed node
            EventConditionValueChangedStartNode valueChangedNode;
            // Ensure the start node is an event node
            if (_startNode is not EventConditionValueChangedStartNode node)
            {
                valueChangedNode = new EventConditionValueChangedStartNode();
                ReplaceStartNode(valueChangedNode);
            }
            else
            {
                valueChangedNode = node;
            }

            valueChangedNode.UpdateOutputPins(EventPath);
        }

        if (!Script.Nodes.Contains(_startNode))
            Script.AddNode(_startNode);
        Script.Save();
    }

    /// <summary>
    ///     Gets the start node of the event script, if any
    /// </summary>
    /// <returns>The start node of the event script, if any.</returns>
    public INode GetStartNode()
    {
        return _startNode;
    }

    private void ReplaceStartNode(IEventConditionNode newStartNode)
    {
        if (Script.Nodes.Contains(_startNode))
            Script.RemoveNode(_startNode);

        _startNode = newStartNode;
        if (!Script.Nodes.Contains(_startNode))
            Script.AddNode(_startNode);
    }

    private bool Evaluate()
    {
        if (EventPath == null)
            return false;

        object? value = EventPath.GetValue();
        if (_startNode is EventConditionEventStartNode)
        {
            if (value is not IDataModelEvent dataModelEvent || dataModelEvent.LastTrigger <= _lastProcessedTrigger)
                return false;

            _lastProcessedTrigger = dataModelEvent.LastTrigger;
        }
        else if (_startNode is EventConditionValueChangedStartNode valueChangedNode)
        {
            if (Equals(value, _lastProcessedValue))
                return false;

            valueChangedNode.UpdateValues(value, _lastProcessedValue);
            _lastProcessedValue = value;
        }

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
            if (IsMet && !_wasMet)
                ProfileElement.Timeline.JumpToStart();
            if (!IsMet && _wasMet && ToggleOffMode == EventToggleOffMode.SkipToEnd)
                ProfileElement.Timeline.JumpToEndSegment();
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
                else if (OverlapMode == EventOverlapMode.Copy && ProfileElement is Layer layer && layer.Parent is not Layer)
                    layer.CreateRenderCopy(10);
            }
        }

        // Stick to mean segment in toggle mode for as long as the condition is met
        ProfileElement.Timeline.Update(TimeSpan.FromSeconds(deltaTime), TriggerMode == EventTriggerMode.Toggle && IsMet);
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
        ToggleOffMode = (EventToggleOffMode) _entity.ToggleOffMode;

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
        _entity.ToggleOffMode = (int) ToggleOffMode;

        // If the exit node isn't connected and there are only the start- and exit node, don't save the script
        if (!Script.ExitNodeConnected && Script.Nodes.Count() <= 2)
        {
            _entity.Script = null;
        }
        else
        {
            Script.Save();
            _entity.Script = Script.Entity;
        }

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
        INode? existingEventNode = Script.Nodes.FirstOrDefault(n => n.Id == EventConditionEventStartNode.NodeId || n.Id == EventConditionValueChangedStartNode.NodeId);
        if (existingEventNode != null)
            _startNode = (IEventConditionNode) existingEventNode;

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

/// <summary>
///     Represents a mode for render elements when toggling off the event when using <see cref="EventTriggerMode.Toggle" />
///     .
/// </summary>
public enum EventToggleOffMode
{
    /// <summary>
    ///     When the event toggles the condition off, finish the the current run of the main timeline
    /// </summary>
    Finish,

    /// <summary>
    ///     When the event toggles the condition off, skip to the end segment of the timeline
    /// </summary>
    SkipToEnd
}