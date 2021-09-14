using System;
using System.Linq;
using Artemis.Core.Internal;
using Artemis.Storage.Entities.Profile.Conditions;

namespace Artemis.Core
{
    public class EventCondition : CorePropertyChanged, IDisposable, IStorageModel
    {
        private readonly string _displayName;
        private readonly object? _context;
        private DateTime _lastProcessedTrigger;
        private DataModelPath? _eventPath;

        internal EventCondition(string displayName, object? context)
        {
            _displayName = displayName;
            _context = context;

            Entity = new EventConditionEntity();
            Script = new NodeScript<bool>($"Activate {displayName}", $"Whether or not the event should activate the {displayName}", context);
            UpdateEventNode();
        }

        internal EventCondition(EventConditionEntity entity, string displayName, object? context)
        {
            _displayName = displayName;
            _context = context;

            Entity = entity;
            Script = null!;

            Load();
        }

        /// <summary>
        ///     Gets the script that drives the event condition
        /// </summary>
        public NodeScript<bool> Script { get; private set; }

        /// <summary>
        ///     Gets or sets the path to the event that drives this event condition
        /// </summary>
        public DataModelPath? EventPath
        {
            set => SetAndNotify(ref _eventPath, value);
            get => _eventPath;
        }

        internal EventConditionEntity Entity { get; }

        internal bool Evaluate()
        {
            if (EventPath?.GetValue() is not IDataModelEvent dataModelEvent || dataModelEvent.LastTrigger <= _lastProcessedTrigger)
                return false;

            // TODO: Place dataModelEvent.LastEventArgumentsUntyped; in the start node
            Script.Run();

            _lastProcessedTrigger = dataModelEvent.LastTrigger;

            return Script.Result;
        }

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            Script.Dispose();
            EventPath?.Dispose();
        }

        #endregion

        internal void LoadNodeScript()
        {
            Script.Load();
            UpdateEventNode();
        }

        /// <summary>
        /// Updates the event node, applying the selected event
        /// </summary>
        public void UpdateEventNode()
        {
            if (EventPath?.GetValue() is not IDataModelEvent dataModelEvent)
                return;

            if (Script.Nodes.FirstOrDefault(n => n is EventStartNode) is EventStartNode existing)
                existing.UpdateDataModelEvent(dataModelEvent);
            else
            {
                EventStartNode node = new();
                node.UpdateDataModelEvent(dataModelEvent);
                Script.AddNode(node);
            }
        }

        #region Implementation of IStorageModel

        /// <inheritdoc />
        public void Load()
        {
            EventPath = new DataModelPath(null, Entity.EventPath);
            Script = new NodeScript<bool>($"Activate {_displayName}", $"Whether or not the event should activate the {_displayName}", Entity.Script, _context);
            UpdateEventNode();
        }

        /// <inheritdoc />
        public void Save()
        {
            EventPath?.Save();
            Entity.EventPath = EventPath?.Entity;
            Script.Save();
            Entity.Script = Script.Entity;
        }

        #endregion
    }
}