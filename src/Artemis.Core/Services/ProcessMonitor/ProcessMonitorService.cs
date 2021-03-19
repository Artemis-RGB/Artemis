using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;

namespace Artemis.Core.Services
{
    internal class ProcessMonitorService : IProcessMonitorService
    {
        private readonly ILogger _logger;
        private readonly Timer _processScanTimer;
        private readonly ProcessComparer _comparer;
        private Process[] _lastScannedProcesses;

        public ProcessMonitorService(ILogger logger)
        {
            _logger = logger;
            _lastScannedProcesses = Process.GetProcesses();
            _processScanTimer = new Timer(1000);
            _processScanTimer.Elapsed += OnTimerElapsed;
            _processScanTimer.Start();
            _comparer = new ProcessComparer();
        }

        public event EventHandler<ProcessEventArgs>? ProcessStarted;

        public event EventHandler<ProcessEventArgs>? ProcessStopped;

        public IEnumerable<Process> GetRunningProcesses()
        {
            return _lastScannedProcesses;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Process[] newProcesses = Process.GetProcesses();
            foreach (Process startedProcess in newProcesses.Except(_lastScannedProcesses, _comparer))
            {
                ProcessStarted?.Invoke(this, new ProcessEventArgs(startedProcess));
                _logger.Debug("Started Process: {startedProcess}", startedProcess.ProcessName);
            }

            foreach (Process stoppedProcess in _lastScannedProcesses.Except(newProcesses, _comparer))
            {
                ProcessStopped?.Invoke(this, new ProcessEventArgs(stoppedProcess));
                _logger.Debug("Stopped Process: {stoppedProcess}", stoppedProcess.ProcessName);
            }

            _lastScannedProcesses = newProcesses;
        }
    }

    internal class ProcessComparer : IEqualityComparer<Process>
    {
        public bool Equals(Process? x, Process? y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.Id == y.Id && x.ProcessName == y.ProcessName && x.SessionId == y.SessionId;
        }

        public int GetHashCode(Process obj)
        {
            if (obj == null) return 0;
            return obj.Id;
        }
    }
}
