using System;
using System.Collections.Generic;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Class that provides info on running processes, including events on event start and stop
    /// </summary>
    public abstract class ProcessWatcher
    {
        public event EventHandler<ProcessEventArgs> ProcessStarted;
        public event EventHandler<ProcessEventArgs> ProcessStopped;

        public abstract IEnumerable<string> GetRunningProcesses();

        public virtual void OnProcessStarted(string processName)
        {
            ProcessStarted?.Invoke(this, new ProcessEventArgs(processName));
        }

        public virtual void OnProcessStopped(string processName)
        {
            ProcessStopped?.Invoke(this, new ProcessEventArgs(processName));
        }
    }
}