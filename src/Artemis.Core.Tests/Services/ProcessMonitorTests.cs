using System.Collections.Immutable;
using System.Diagnostics;
using Artemis.Core.Services;

namespace Artemis.Core.Tests.Services;

/// <summary>
///     Runs the real process monitor against the processes on this machine, scanning every 50ms instead of every second.
/// </summary>
public sealed class ProcessMonitorTests : IDisposable
{
    private readonly bool _wasStarted;
    private readonly TimeSpan _updateInterval;

    public ProcessMonitorTests()
    {
        _wasStarted = ProcessMonitor.IsStarted;
        _updateInterval = ProcessMonitor.UpdateInterval;
        ProcessMonitor.Start();
        ProcessMonitor.UpdateInterval = TimeSpan.FromMilliseconds(50);
        WaitForScan();
    }

    public void Dispose()
    {
        ProcessMonitor.UpdateInterval = _updateInterval;
        if (!_wasStarted)
            ProcessMonitor.Stop();
    }

    [Fact]
    public void Processes_StaysTheSameWhileNoProcessStartsOrStops()
    {
        int changes = 0;
        EventHandler<ProcessEventArgs> onChange = (_, _) => Interlocked.Increment(ref changes);
        ProcessMonitor.ProcessStarted += onChange;
        ProcessMonitor.ProcessStopped += onChange;
        try
        {
            // Other processes on the machine come and go, so try until a scan passes without any of them changing
            for (int attempt = 0; attempt < 20; attempt++)
            {
                int changesBefore = Volatile.Read(ref changes);
                ImmutableArray<ProcessInfo> before = ProcessMonitor.Processes;
                WaitForScan();
                ImmutableArray<ProcessInfo> after = ProcessMonitor.Processes;
                if (Volatile.Read(ref changes) != changesBefore)
                    continue;

                (after == before).ShouldBeTrue("A scan without changes should keep the same array");
                return;
            }

            Assert.Fail("Every scan saw a process start or stop");
        }
        finally
        {
            ProcessMonitor.ProcessStarted -= onChange;
            ProcessMonitor.ProcessStopped -= onChange;
        }
    }

    [Fact]
    public void Processes_FollowsProcessesStartingAndStopping()
    {
        using Process process = Process.Start(OperatingSystem.IsWindows()
            ? new ProcessStartInfo("ping", "-n 30 127.0.0.1") {CreateNoWindow = true, UseShellExecute = false}
            : new ProcessStartInfo("sleep", "30") {UseShellExecute = false})!;
        try
        {
            WaitForScan();
            ProcessMonitor.Processes.ShouldContain(p => p.ProcessId == process.Id);
        }
        finally
        {
            process.Kill();
            process.WaitForExit();
        }

        WaitForScan();
        ProcessMonitor.Processes.ShouldNotContain(p => p.ProcessId == process.Id);
    }

    // Waits for two scans to finish, the first may have started before the caller changed anything
    private static void WaitForScan()
    {
        for (int i = 0; i < 2; i++)
        {
            DateTime lastUpdate = ProcessMonitor.LastUpdate;
            SpinWait.SpinUntil(() => ProcessMonitor.LastUpdate > lastUpdate, TimeSpan.FromSeconds(5)).ShouldBeTrue("The process monitor didn't scan");
        }
    }
}
