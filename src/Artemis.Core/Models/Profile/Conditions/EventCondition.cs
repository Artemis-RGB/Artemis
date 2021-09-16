using System;
using System.Linq;
using Artemis.Core.Internal;
using Artemis.Storage.Entities.Profile.Abstract;
using Artemis.Storage.Entities.Profile.Conditions;

namespace Artemis.Core
{
    public class EventCondition : CorePropertyChanged, INodeScriptCondition
    {
        private readonly string _displayName;
        private readonly EventConditionEntity _entity;
        private EventDefaultNode? _eventNode;
        private TimeLineEventOverlapMode _eventOverlapMode;
        private DataModelPath? _eventPath;
        private DateTime _lastProcessedTrigger;
        private NodeScript<bool>? _script;

        /// <summary>
        ///     Creates a new instance of the <see cref="EventCondition" /> class
        /// </summary>
        public EventCondition(ProfileElement profileElement)
        {
            _entity = new EventConditionEntity();
            _displayName = profileElement.GetType().Name;

            ProfileElement = profileElement;
        }

        internal EventCondition(EventConditionEntity entity, ProfileElement profileElement)
        {
            _entity = entity;
            _displayName = profileElement.GetType().Name;

            ProfileElement = profileElement;
            Load();
        }

        /// <summary>
        ///     Gets the script that drives the event condition
        /// </summary>
        public NodeScript<bool>? Script
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
        ///     Gets or sets how the condition behaves when events trigger before the timeline finishes
        /// </summary>
        public TimeLineEventOverlapMode EventOverlapMode
        {
            get => _eventOverlapMode;
            set => SetAndNotify(ref _eventOverlapMode, value);
        }

        /// <summary>
        ///     Updates the event node, applying the selected event
        /// </summary>
        public void UpdateEventNode()
        {
            if (Script == null || EventPath?.GetValue() is not IDataModelEvent dataModelEvent)
                return;

            if (Script.Nodes.FirstOrDefault(n => n is EventDefaultNode) is EventDefaultNode existing)
            {
                existing.UpdateDataModelEvent(dataModelEvent);
                _eventNode = existing;
            }
            else
            {
                _eventNode = new EventDefaultNode() {X = -300};
                _eventNode.UpdateDataModelEvent(dataModelEvent);
            }

            if (_eventNode.Pins.Any() && !Script.Nodes.Contains(_eventNode))
                Script.AddNode(_eventNode);
            else
                Script.RemoveNode(_eventNode);
        }

        /// <summary>
        ///     Updates the <see cref="Script" /> with a new empty node script
        /// </summary>
        public void CreateEmptyNodeScript()
        {
            Script?.Dispose();
            Script = new NodeScript<bool>($"Activate {_displayName}", $"Whether or not the event should activate the {_displayName}", ProfileElement.Profile);
            UpdateEventNode();
        }

        private bool Evaluate()
        {
            if (EventPath?.GetValue() is not IDataModelEvent dataModelEvent || dataModelEvent.LastTrigger <= _lastProcessedTrigger)
                return false;

            _lastProcessedTrigger = dataModelEvent.LastTrigger;

            if (Script == null)
                return true;

            Script.Run();
            return Script.Result;
        }

        /// <inheritdoc />
        public IConditionEntity Entity => _entity;

        /// <inheritdoc />
        public ProfileElement ProfileElement { get; }

        /// <inheritdoc />
        public bool IsMet { get; private set; }

        /// <inheritdoc />
        public void Update()
        {
            if (EventOverlapMode == TimeLineEventOverlapMode.Toggle)
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
        public void ApplyToTimeline(bool isMet, bool wasMet, Timeline timeline)
        {
            if (!isMet)
            {
                if (EventOverlapMode == TimeLineEventOverlapMode.Toggle)
                    timeline.JumpToEnd();
                return;
            }

            // Event overlap mode doesn't apply in this case
            if (timeline.IsFinished)
            {
                timeline.JumpToStart();
                return;
            }

            // If the timeline was already running, look at the event overlap mode
            if (EventOverlapMode == TimeLineEventOverlapMode.Restart)
                timeline.JumpToStart();
            else if (EventOverlapMode == TimeLineEventOverlapMode.Copy)
                timeline.AddExtraTimeline();
            else if (EventOverlapMode == TimeLineEventOverlapMode.Toggle && !wasMet)
                timeline.JumpToStart();

            // The remaining overlap mode is 'ignore' which requires no further action
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
            EventOverlapMode = (TimeLineEventOverlapMode) _entity.EventOverlapMode;
            if (_entity.Script != null)
                Script = new NodeScript<bool>($"Activate {_displayName}", $"Whether or not the event should activate the {_displayName}", _entity.Script, ProfileElement.Profile);
            if (_entity.EventPath != null)
                EventPath = new DataModelPath(_entity.EventPath);
        }

        /// <inheritdoc />
        public void Save()
        {
            _entity.EventOverlapMode = (int) EventOverlapMode;
            Script?.Save();
            _entity.Script = Script?.Entity;
            EventPath?.Save();
            _entity.EventPath = EventPath?.Entity;
        }

        /// <inheritdoc />
        public void LoadNodeScript()
        {
            if (Script == null)
                return;

            Script.Load();
            UpdateEventNode();
            Script.LoadConnections();
        }

        #endregion
    }
}