using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;

namespace Artemis.Core.Services
{
    public class ProcessMonitorService : IProcessMonitorService
    {
        private readonly ILogger _logger;
        private readonly Timer _processScanTimer;
        private Process[] _lastScannedProcesses;

        public ProcessMonitorService(ILogger logger)
        {
            _logger = logger;
            _lastScannedProcesses = Process.GetProcesses().DistinctBy(p => p.ProcessName).ToArray();
            _processScanTimer = new Timer(1000);
            _processScanTimer.Elapsed += OnTimerElapsed;
            _processScanTimer.Start();
        }

        public event EventHandler<ProcessEventArgs> ProcessStarted;

        public event EventHandler<ProcessEventArgs> ProcessStopped;

        public IEnumerable<Process> GetRunningProcesses()
        {
            return _lastScannedProcesses;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            var newProcesses = Process.GetProcesses().DistinctBy(p => p.ProcessName).ToArray();
            foreach (var startedProcess in newProcesses.Except(_lastScannedProcesses, new ProcessComparer()))
            {
                ProcessStarted?.Invoke(this, new ProcessEventArgs(startedProcess));
                _logger.Debug("Started Process: {startedProcess}", startedProcess.ProcessName);
            }

            foreach (var stoppedProcess in _lastScannedProcesses.Except(newProcesses, new ProcessComparer()))
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
