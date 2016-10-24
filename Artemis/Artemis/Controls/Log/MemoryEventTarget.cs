using System;
using NLog;
using NLog.Targets;

namespace Artemis.Controls.Log
{
    public class MemoryEventTarget : Target
    {
        public event Action<LogEventInfo> EventReceived;

        /// <summary>
        ///     Notifies listeners about new event
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            EventReceived?.Invoke(logEvent);
        }
    }
}