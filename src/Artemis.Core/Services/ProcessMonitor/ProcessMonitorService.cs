using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Artemis.Core.Services
{
    public class ProcessMonitorService : IProcessMonitorService
    {
        private readonly List<ProcessWatcher> _watchers = new List<ProcessWatcher>();
        public event EventHandler<ProcessEventArgs> ProcessStarted;
        public event EventHandler<ProcessEventArgs> ProcessStopped;

        public void AddProcessWatcher(ProcessWatcher watcher)
        {
            watcher.ProcessStarted += ProviderOnProcessStarted;
            watcher.ProcessStopped += ProviderOnProcessStopped;
            _watchers.Add(watcher);
        }

        public void RemoveProcessWatcher(ProcessWatcher watcher)
        {
            if (!_watchers.Contains(watcher))
                return;

            _watchers.Remove(watcher);
            watcher.ProcessStarted -= ProviderOnProcessStarted;
            watcher.ProcessStopped -= ProviderOnProcessStopped;
        }

        public IEnumerable<string> GetRunningProcesses()
        {
            return _watchers.SelectMany(w => w.GetRunningProcesses());
        }

        private void ProviderOnProcessStopped(object? sender, ProcessEventArgs e)
        {
            ProcessStopped?.Invoke(this, e);
        }

        private void ProviderOnProcessStarted(object? sender, ProcessEventArgs e)
        {
            ProcessStarted?.Invoke(this, e);
        }
    }
}
