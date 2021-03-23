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
        private IDataModelEvent? _valueChangedEvent;

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

        /// <summary>
        ///     Gets the last time the event this condition is applied to was triggered
        /// </summary>
        public DateTime LastTrigger { get; private set; }

        /// <summary>
        ///     Gets or sets the type of argument the event provides
        /// </summary>
        public Type? EventArgumentType { get; private set; }

        internal DataModelConditionEventEntity Entity { get; set; }

        /// <inheritdoc />
        public override bool Evaluate()
        {
            if (_disposed)
                throw new ObjectDisposedException("DataModelConditionEvent");

            IDataModelEvent? dataModelEvent = GetDataModelEvent();
            if (dataModelEvent == null)
                return false;
            dataModelEvent.Update();

            // Only evaluate to true once every time the event has been triggered since the last evaluation
            if (dataModelEvent.LastTrigger <= LastTrigger)
                return false;

            LastTrigger = DateTime.Now;

            // If there is a child (root group), it must evaluate to true whenever the event triggered
            if (Children.Any())
                return Children[0].EvaluateObject(dataModelEvent.LastEventArgumentsUntyped);

            // If there are no children, we always evaluate to true whenever the event triggered
            return true;
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
            CreateValueChangedEventIfNeeded();

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

            LastTrigger = GetDataModelEvent()?.LastTrigger ?? DateTime.Now;
        }

        /// <summary>
        ///     Returns the <see cref="IDataModelEvent" /> this <see cref="DataModelConditionEvent" /> is triggered by
        /// </summary>
        /// <returns>The <see cref="IDataModelEvent" /> this <see cref="DataModelConditionEvent" /> is triggered by</returns>
        public IDataModelEvent? GetDataModelEvent()
        {
            if (_valueChangedEvent != null)
                return _valueChangedEvent;
            return EventPath?.GetValue() as IDataModelEvent;
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

        internal override bool EvaluateObject(object? target)
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

            DataModelPath eventPath = new(null, Entity.EventPath);
            EventPath = eventPath;
            SubscribeToEventPath();
            CreateValueChangedEventIfNeeded();

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

            LastTrigger = GetDataModelEvent()?.LastTrigger ?? DateTime.Now;
        }

        private Type? GetEventArgumentType()
        {
            if (EventPath == null || !EventPath.IsValid)
                return null;

            if (_valueChangedEvent != null)
                return _valueChangedEvent.ArgumentsType;

            // Cannot rely on EventPath.GetValue() because part of the path might be null
            Type eventType = EventPath.GetPropertyType()!;
            return eventType.IsGenericType ? eventType.GetGenericArguments()[0] : typeof(DataModelEventArgs);
        }

        private void SubscribeToEventPath()
        {
            if (EventPath == null) return;
            EventPath.PathValidated += EventPathOnPathValidated;
            EventPath.PathInvalidated += EventPathOnPathInvalidated;
        }

        private void CreateValueChangedEventIfNeeded()
        {
            Type? propertyType = EventPath?.GetPropertyType();
            if (propertyType == null)
                return;

            if (!typeof(IDataModelEvent).IsAssignableFrom(propertyType))
            {
                IDataModelEvent? instance = (IDataModelEvent?) Activator.CreateInstance(typeof(DataModelValueChangedEvent<>).MakeGenericType(propertyType), EventPath);
                _valueChangedEvent = instance ?? throw new ArtemisCoreException("Failed to create a DataModelValueChangedEvent for a property changed data model event");
            }
            else
            {
                _valueChangedEvent = null;
            }
        }

        #region Event handlers

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