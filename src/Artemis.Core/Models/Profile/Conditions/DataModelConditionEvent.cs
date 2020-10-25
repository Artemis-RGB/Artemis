using System;
using System.Linq;
using Artemis.Storage.Entities.Profile.Abstract;
using Artemis.Storage.Entities.Profile.Conditions;

namespace Artemis.Core
{
    /// <summary>
    ///     A condition that evaluates to true when an event is triggered
    /// </summary>
    public class DataModelConditionEvent : DataModelConditionPart
    {
        private bool _disposed;
        private bool _reinitializing;
        private IDataModelEvent? _event;
        private bool _eventTriggered;

        /// <summary>
        ///     Creates a new instance of the <see cref="DataModelConditionEvent" /> class
        /// </summary>
        /// <param name="parent"></param>
        public DataModelConditionEvent(DataModelConditionPart parent)
        {
            Parent = parent;
            Entity = new DataModelConditionEventEntity();

            Initialize();
        }

        internal DataModelConditionEvent(DataModelConditionPart parent, DataModelConditionEventEntity entity)
        {
            Parent = parent;
            Entity = entity;

            Initialize();
        }

        /// <summary>
        ///     Gets the path of the event property
        /// </summary>
        public DataModelPath? EventPath { get; private set; }

        public Type? EventArgumentType { get; set; }

        internal DataModelConditionEventEntity Entity { get; set; }
        
        /// <inheritdoc />
        public override bool Evaluate()
        {
            if (_disposed)
                throw new ObjectDisposedException("DataModelConditionEvent");

            // Ensure the event has not been replaced
            if (EventPath?.GetValue() is IDataModelEvent dataModelEvent && _event != dataModelEvent)
                SubscribeToDataModelEvent(dataModelEvent);

            // Only evaluate to true once every time the event has been triggered
            if (!_eventTriggered)
                return false;

            _eventTriggered = false;

            // If there is a child (root group), it must evaluate to true whenever the event triggered
            if (Children.Any())
                return Children[0].Evaluate();
            // If there are no children, we always evaluate to true whenever the event triggered
            return true;
        }

        private void SubscribeToDataModelEvent(IDataModelEvent dataModelEvent)
        {
            if (_event != null)
                _event.EventTriggered -= OnEventTriggered;

            _event = dataModelEvent;
            if (_event != null)
                _event.EventTriggered += OnEventTriggered;
        }

        /// <summary>
        ///     Updates the event the condition is triggered by
        /// </summary>
        public void UpdateEvent(DataModelPath? path)
        {
            if (_disposed)
                throw new ObjectDisposedException("DataModelConditionEvent");

            if (path != null && !path.IsValid)
                throw new ArtemisCoreException("Cannot update event to an invalid path");

            EventPath?.Dispose();
            EventPath = path != null ? new DataModelPath(path) : null;
            SubscribeToEventPath();

            // Remove the old root group that was tied to the old data model
            ClearChildren();

            if (EventPath != null)
            {
                EventArgumentType = GetEventArgumentType();
                // Create a new root group
                AddChild(new DataModelConditionGroup(this));
            }
            else
            {
                EventArgumentType = null;
            }
        }

        private Type? GetEventArgumentType()
        {
            if (EventPath == null || !EventPath.IsValid)
                return null;

            // Cannot rely on EventPath.GetValue() because part of the path might be null
            Type eventType = EventPath.GetPropertyType()!;
            return eventType.IsGenericType ? eventType.GetGenericArguments()[0] : typeof(DataModelEventArgs);
        }

        #region IDisposable

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            _disposed = true;

            EventPath?.Dispose();

            foreach (DataModelConditionPart child in Children)
                child.Dispose();

            base.Dispose(disposing);
        }

        #endregion

        internal override bool EvaluateObject(object target)
        {
            return false;
        }

        internal override void Save()
        {
            // Don't save an invalid state
            if (EventPath != null && !EventPath.IsValid)
                return;

            // Target list
            EventPath?.Save();
            Entity.EventPath = EventPath?.Entity;

            // Children
            Entity.Children.Clear();
            Entity.Children.AddRange(Children.Select(c => c.GetEntity()));
            foreach (DataModelConditionPart child in Children)
                child.Save();
        }

        internal override DataModelConditionPartEntity GetEntity()
        {
            return Entity;
        }

        internal void Initialize()
        {
            ClearChildren();

            if (Entity.EventPath == null)
                return;

            // Ensure the list path is valid and points to a list
            DataModelPath eventPath = new DataModelPath(null, Entity.EventPath);
            // Can't check this on an invalid list, if it becomes valid later lets hope for the best
            if (eventPath.IsValid && !PointsToEvent(eventPath))
                return;

            EventPath = eventPath;
            SubscribeToEventPath();

            EventArgumentType = GetEventArgumentType();
            // There should only be one child and it should be a group
            if (Entity.Children.FirstOrDefault() is DataModelConditionGroupEntity rootGroup)
            {
                AddChild(new DataModelConditionGroup(this, rootGroup));
            }
            else
            {
                Entity.Children.Clear();
                AddChild(new DataModelConditionGroup(this));
            }
        }

        private bool PointsToEvent(DataModelPath dataModelPath)
        {
            Type? type = dataModelPath.GetPropertyType();
            if (type == null)
                return false;

            return typeof(IDataModelEvent).IsAssignableFrom(type);
        }

        private void SubscribeToEventPath()
        {
            if (EventPath == null) return;
            EventPath.PathValidated += EventPathOnPathValidated;
            EventPath.PathInvalidated += EventPathOnPathInvalidated;
        }

        #region Event handlers

        private void OnEventTriggered(object? sender, EventArgs e)
        {
            _eventTriggered = true;
        }

        private void EventPathOnPathValidated(object? sender, EventArgs e)
        {
            if (_reinitializing)
                return;

            _reinitializing = true;
            EventPath?.Dispose();
            Initialize();
            _reinitializing = false;
        }

        private void EventPathOnPathInvalidated(object? sender, EventArgs e)
        {
            if (_reinitializing)
                return;

            _reinitializing = true;
            EventPath?.Dispose();
            Initialize();
            _reinitializing = false;
        }

        #endregion
    }
}