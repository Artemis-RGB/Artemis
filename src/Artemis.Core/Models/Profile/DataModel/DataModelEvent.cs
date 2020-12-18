using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.DataModelExpansions;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a data model event with event arguments of type <typeparamref name="T" />
    /// </summary>
    public class DataModelEvent<T> : IDataModelEvent where T : DataModelEventArgs
    {
        private bool _trackHistory;

        /// <summary>
        ///     Creates a new instance of the <see cref="DataModelEvent{T}" /> class with history tracking disabled
        /// </summary>
        public DataModelEvent()
        {
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="DataModelEvent{T}" />
        /// </summary>
        /// <param name="trackHistory">A boolean indicating whether the last 20 events should be tracked</param>
        public DataModelEvent(bool trackHistory)
        {
            _trackHistory = trackHistory;
        }

        /// <inheritdoc />
        [DataModelProperty(Name = "Last event trigger", Description = "The time at which the event last triggered")]
        public DateTime LastTrigger { get; private set; }

        /// <summary>
        ///     Gets the event arguments of the last time the event was triggered
        /// </summary>
        [DataModelProperty(Description = "The arguments of the last time this event triggered")]
        public T? LastEventArguments { get; private set; }

        /// <inheritdoc />
        [DataModelProperty(Description = "The total amount of times this event has triggered since the module was activated")]
        public int TriggerCount { get; private set; }

        /// <summary>
        ///     Gets a queue of the last 20 event arguments
        ///     <para>Always empty if <see cref="TrackHistory" /> is <see langword="false" /></para>
        /// </summary>
        [DataModelProperty(Description = "The arguments of the last time this event triggered")]
        public Queue<T> EventArgumentsHistory { get; } = new(20);

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

            if (TrackHistory)
            {
                lock (EventArgumentsHistory)
                {
                    if (EventArgumentsHistory.Count == 20)
                        EventArgumentsHistory.Dequeue();
                    EventArgumentsHistory.Enqueue(eventArgs);
                }
            }

            OnEventTriggered();
        }

        internal virtual void OnEventTriggered()
        {
            EventTriggered?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        [DataModelIgnore]
        public Type ArgumentsType => typeof(T);

        /// <inheritdoc />
        [DataModelIgnore]
        public bool TrackHistory
        {
            get => _trackHistory;
            set
            {
                EventArgumentsHistory.Clear();
                _trackHistory = value;
            }
        }

        /// <inheritdoc />
        [DataModelIgnore]
        public DataModelEventArgs? LastEventArgumentsUntyped => LastEventArguments;

        /// <inheritdoc />
        [DataModelIgnore]
        public List<DataModelEventArgs> EventArgumentsHistoryUntyped => EventArgumentsHistory.Cast<DataModelEventArgs>().ToList();

        /// <inheritdoc />
        public event EventHandler? EventTriggered;

        /// <inheritdoc />
        public void Reset()
        {
            TriggerCount = 0;
            EventArgumentsHistory.Clear();
        }
    }

    /// <summary>
    ///     Represents a data model event without event arguments
    /// </summary>
    public class DataModelEvent : IDataModelEvent
    {
        private bool _trackHistory;

        /// <summary>
        ///     Creates a new instance of the <see cref="DataModelEvent" /> class with history tracking disabled
        /// </summary>
        public DataModelEvent()
        {
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="DataModelEvent" />
        /// </summary>
        /// <param name="trackHistory">A boolean indicating whether the last 20 events should be tracked</param>
        public DataModelEvent(bool trackHistory)
        {
            _trackHistory = trackHistory;
        }

        /// <inheritdoc />
        [DataModelProperty(Name = "Last event trigger", Description = "The time at which the event last triggered")]
        public DateTime LastTrigger { get; private set; }

        /// <summary>
        ///     Gets the event arguments of the last time the event was triggered
        /// </summary>
        [DataModelProperty(Description = "The arguments of the last time this event triggered")]
        public DataModelEventArgs? LastEventArguments { get; private set; }

        /// <inheritdoc />
        [DataModelProperty(Description = "The total amount of times this event has triggered since the module was activated")]
        public int TriggerCount { get; private set; }

        /// <summary>
        ///     Gets a queue of the last 20 event arguments
        ///     <para>Always empty if <see cref="TrackHistory" /> is <see langword="false" /></para>
        /// </summary>
        [DataModelProperty(Description = "The arguments of the last time this event triggered")]
        public Queue<DataModelEventArgs> EventArgumentsHistory { get; } = new(20);

        /// <summary>
        ///     Trigger the event
        /// </summary>
        public void Trigger()
        {
            DataModelEventArgs eventArgs = new() {TriggerTime = DateTime.Now};

            LastEventArguments = eventArgs;
            LastTrigger = DateTime.Now;
            TriggerCount++;

            if (TrackHistory)
            {
                lock (EventArgumentsHistory)
                {
                    if (EventArgumentsHistory.Count == 20)
                        EventArgumentsHistory.Dequeue();
                    EventArgumentsHistory.Enqueue(eventArgs);
                }
            }

            OnEventTriggered();
        }

        internal virtual void OnEventTriggered()
        {
            EventTriggered?.Invoke(this, EventArgs.Empty);
        }
        
        /// <inheritdoc />
        [DataModelIgnore]
        public Type ArgumentsType => typeof(DataModelEventArgs);

        /// <inheritdoc />
        [DataModelIgnore]
        public bool TrackHistory
        {
            get => _trackHistory;
            set
            {
                EventArgumentsHistory.Clear();
                _trackHistory = value;
            }
        }

        /// <inheritdoc />
        [DataModelIgnore]
        public DataModelEventArgs? LastEventArgumentsUntyped => LastEventArguments;

        /// <inheritdoc />
        [DataModelIgnore]
        public List<DataModelEventArgs> EventArgumentsHistoryUntyped => EventArgumentsHistory.ToList();

        /// <inheritdoc />
        public event EventHandler? EventTriggered;

        /// <inheritdoc />
        public void Reset()
        {
            TriggerCount = 0;
            EventArgumentsHistory.Clear();
        }
    }
}