using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Artemis.Core.Modules;
using Serilog;

namespace Artemis.Core.Services;

internal class ProcessMonitorService : IProcessMonitorService
{
    private readonly ProcessComparer _comparer;
    private Process[] _lastScannedProcesses;

    public ProcessMonitorService()
    {
        _comparer = new ProcessComparer();
        _lastScannedProcesses = Process.GetProcesses();
        Timer processScanTimer = new(1000);
        processScanTimer.Elapsed += OnTimerElapsed;
        processScanTimer.Start();

        ProcessActivationRequirement.ProcessMonitorService = this;
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        Process[] newProcesses = Process.GetProcesses();
        foreach (Process startedProcess in newProcesses.Except(_lastScannedProcesses, _comparer))
            ProcessStarted?.Invoke(this, new ProcessEventArgs(startedProcess));
        foreach (Process stoppedProcess in _lastScannedProcesses.Except(newProcesses, _comparer))
            ProcessStopped?.Invoke(this, new ProcessEventArgs(stoppedProcess));

        _lastScannedProcesses = newProcesses;
    }

    public event EventHandler<ProcessEventArgs>? ProcessStarted;
    public event EventHandler<ProcessEventArgs>? ProcessStopped;

    public IEnumerable<Process> GetRunningProcesses()
    {
        return _lastScannedProcesses;
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

    public int GetHashCode(Process? obj)
    {
        if (obj == null) return 0;
        return obj.Id;
    }
}