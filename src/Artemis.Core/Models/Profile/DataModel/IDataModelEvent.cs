using System;

namespace Artemis.Core
{
    internal interface IDataModelEvent
    {
        /// <summary>
        ///     Gets the last time the event was triggered
        /// </summary>
        DateTime LastTrigger { get; }

        /// <summary>
        ///     Gets the amount of times the event was triggered
        /// </summary>
        int TriggerCount { get; }

        /// <summary>
        ///     Gets the type of arguments this event contains
        /// </summary>
        Type ArgumentsType { get; }

        /// <summary>
        ///     Fires when the event is triggered
        /// </summary>
        event EventHandler EventTriggered;
    }
}