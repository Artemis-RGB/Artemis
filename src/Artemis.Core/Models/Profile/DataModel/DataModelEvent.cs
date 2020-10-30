using System;
using Artemis.Core.DataModelExpansions;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a data model event with event arguments of type <typeparamref name="T" />
    /// </summary>
    public class DataModelEvent<T> : IDataModelEvent where T : DataModelEventArgs
    {
        /// <summary>
        ///     Trigger the event with the given <paramref name="eventArgs" />
        /// </summary>
        /// <param name="eventArgs">The event argument to pass to the event</param>
        public void Trigger(T eventArgs)
        {
            if (eventArgs == null) throw new ArgumentNullException(nameof(eventArgs));
            eventArgs.TriggerTime = DateTime.Now;

            LastEventArguments = eventArgs;
            LastTrigger = DateTime.Now;
            TriggerCount++;

            OnEventTriggered();
        }

        internal virtual void OnEventTriggered()
        {
            EventTriggered?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        public DateTime LastTrigger { get; private set; }

        /// <inheritdoc />
        public int TriggerCount { get; private set; }

        /// <summary>
        ///     Gets the event arguments of the last time the event was triggered
        /// </summary>
        public T? LastEventArguments { get; private set; }

        /// <inheritdoc />
        [DataModelIgnore]
        public Type ArgumentsType => typeof(T);

        /// <inheritdoc />
        [DataModelIgnore]
        public DataModelEventArgs? LastEventArgumentsUntyped => LastEventArguments;

        /// <inheritdoc />
        public event EventHandler? EventTriggered;
    }

    /// <summary>
    ///     Represents a data model event without event arguments
    /// </summary>
    public class DataModelEvent : IDataModelEvent
    {
        /// <summary>
        ///     Trigger the event
        /// </summary>
        public void Trigger()
        {
            DataModelEventArgs eventArgs = new DataModelEventArgs {TriggerTime = DateTime.Now};

            LastEventArguments = eventArgs;
            LastTrigger = DateTime.Now;
            TriggerCount++;

            OnEventTriggered();
        }

        internal virtual void OnEventTriggered()
        {
            EventTriggered?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        public DateTime LastTrigger { get; private set; }

        /// <inheritdoc />
        public int TriggerCount { get; private set; }

        /// <summary>
        ///     Gets the event arguments of the last time the event was triggered
        /// </summary>
        public DataModelEventArgs? LastEventArguments { get; private set; }

        /// <inheritdoc />
        [DataModelIgnore]
        public Type ArgumentsType => typeof(DataModelEventArgs);

        /// <inheritdoc />
        [DataModelIgnore]
        public DataModelEventArgs? LastEventArgumentsUntyped => LastEventArguments;

        /// <inheritdoc />
        public event EventHandler? EventTriggered;
    }
}