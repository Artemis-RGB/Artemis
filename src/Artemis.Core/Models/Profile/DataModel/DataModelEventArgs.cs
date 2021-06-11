using System;
using Artemis.Core.Modules;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents the base class for data model events that contain event data
    /// </summary>
    public class DataModelEventArgs
    {
        /// <summary>
        ///     Gets the time at which the event with these arguments was triggered
        /// </summary>
        [DataModelIgnore]
        public DateTime TriggerTime { get; internal set; }
    }
}