using System;
using System.Collections.Generic;

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
        ///     Gets or sets a boolean indicating whether the last 20 events should be tracked
        ///     <para>Note: setting this to <see langword="false" /> will clear the current history</para>
        /// </summary>
        bool TrackHistory { get; set; }

        /// <summary>
        ///     Gets the event arguments of the last time the event was triggered by its base type
        /// </summary>
        public DataModelEventArgs? LastEventArgumentsUntyped { get; }

        /// <summary>
        ///     Gets a list of the last 20 event arguments by their base type.
        ///     <para>Always empty if <see cref="TrackHistory" /> is <see langword="false" /></para>
        /// </summary>
        public List<DataModelEventArgs> EventArgumentsHistoryUntyped { get; }

        /// <summary>
        ///     Fires when the event is triggered
        /// </summary>
        event EventHandler EventTriggered;

        /// <summary>
        ///     Resets the trigger count and history of this data model event
        /// </summary>
        void Reset();
    }
}