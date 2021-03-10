using Artemis.Core.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Artemis.UI.ProcessWatchers
{
    public class PollingProcessWatcher : ProcessWatcher
    {
        private readonly ILogger _logger;
        private readonly Timer _processScanTimer;
        private IEnumerable<string> _lastScannedProcesses;

        public PollingProcessWatcher(ILogger logger)
        {
            _logger = logger;
            _lastScannedProcesses = Process.GetProcesses().Select(p => p.ProcessName).Distinct().ToArray();
            _processScanTimer = new Timer(100);
            _processScanTimer.Elapsed += OnTimerElapsed;
            _processScanTimer.Start();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            var newProcesses = Process.GetProcesses().Select(p => p.ProcessName).Distinct();
            foreach (var startedProcess in newProcesses.Except(_lastScannedProcesses))
            {
                OnProcessStarted(startedProcess);
                _logger.Information($"Started Process!: {startedProcess}");
            }

            foreach (var stoppedProcess in _lastScannedProcesses.Except(newProcesses))
            {
                OnProcessStopped(stoppedProcess);
                _logger.Information($"Stopped Process!: {stoppedProcess}");
            }

            _lastScannedProcesses = newProcesses.ToArray();
        }

        public override IEnumerable<string> GetRunningProcesses()
        {
            return _lastScannedProcesses;
        }
    }
}
