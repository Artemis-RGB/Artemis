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
        private IEnumerable<string> _lastScannedProcesses;

        public ProcessMonitorService(ILogger logger)
        {
            _logger = logger;
            _lastScannedProcesses = Process.GetProcesses().Select(p => p.ProcessName).Distinct().ToArray();
            _processScanTimer = new Timer(1000);
            _processScanTimer.Elapsed += OnTimerElapsed;
            _processScanTimer.Start();
        }

        public event EventHandler<ProcessEventArgs> ProcessStarted;

        public event EventHandler<ProcessEventArgs> ProcessStopped;

        public IEnumerable<string> GetRunningProcesses()
        {
            return _lastScannedProcesses;
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            var newProcesses = Process.GetProcesses().Select(p => p.ProcessName).Distinct();
            foreach (var startedProcess in newProcesses.Except(_lastScannedProcesses))
            {
                ProcessStarted?.Invoke(this, new ProcessEventArgs(startedProcess));
                _logger.Verbose("Started Process: {startedProcess}", startedProcess);
            }

            foreach (var stoppedProcess in _lastScannedProcesses.Except(newProcesses))
            {
                ProcessStopped?.Invoke(this, new ProcessEventArgs(stoppedProcess));
                _logger.Verbose("Stopped Process: {stoppedProcess}", stoppedProcess);
            }

            _lastScannedProcesses = newProcesses.ToArray();
        }
    }
}
