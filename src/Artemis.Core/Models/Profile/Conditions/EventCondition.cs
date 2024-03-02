using System;
using System.Collections.Generic;
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
    private NodeScript<bool> _script;
    private DefaultNode _startNode;
    private DataModelPath? _eventPath;
    private DateTime _lastProcessedTrigger;
    private object? _lastProcessedValue;
    private EventOverlapMode _overlapMode;
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
        _script = new NodeScript<bool>($"Activate {_displayName}", $"Whether or not the event should activate the {_displayName}", ProfileElement.Profile, new List<DefaultNode> {_startNode});
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
    
        /// <summary>
    ///     Updates the event node, applying the selected event
    /// </summary>
    public void UpdateEventNode(bool updateScript)
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
                if (updateScript)
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
                if (updateScript)
                    ReplaceStartNode(valueChangedNode);
                _startNode = valueChangedNode;
            }
            else
            {
                valueChangedNode = node;
            }

            valueChangedNode.UpdateOutputPins(EventPath);
        }

        // Script can be null if called before load
        if (!updateScript)
            return;
        if (!Script.Nodes.Contains(_startNode))
        {
            Script.AddNode(_startNode);
            Script.LoadConnections();
        }
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

    private void ReplaceStartNode(DefaultNode newStartNode)
    {
        if (Script.Nodes.Contains(_startNode))
            Script.RemoveNode(_startNode);
        if (!Script.Nodes.Contains(newStartNode))
            Script.AddNode(newStartNode);
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

    #region Storage

    /// <inheritdoc />
    public void Load()
    {
        TriggerMode = (EventTriggerMode) _entity.TriggerMode;
        OverlapMode = (EventOverlapMode) _entity.OverlapMode;
        ToggleOffMode = (EventToggleOffMode) _entity.ToggleOffMode;

        if (_entity.EventPath != null)
            EventPath = new DataModelPath(_entity.EventPath);
        UpdateEventNode(false);
        
        string name = $"Activate {_displayName}";
        string description = $"Whether or not the event should activate the {_displayName}";
        Script = _entity.Script != null
            ? new NodeScript<bool>(name, description, _entity.Script, ProfileElement.Profile, new List<DefaultNode> {_startNode})
            : new NodeScript<bool>(name, description, ProfileElement.Profile, new List<DefaultNode> {_startNode});
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
        UpdateEventNode(true);
        Script.Load();
    }

    #endregion

    #region Implementation of IPluginFeatureDependent

    /// <inheritdoc />
    public List<PluginFeature> GetFeatureDependencies()
    {
        return [..EventPath?.GetFeatureDependencies() ?? [], ..Script.GetFeatureDependencies()];
    }

    #endregion
}