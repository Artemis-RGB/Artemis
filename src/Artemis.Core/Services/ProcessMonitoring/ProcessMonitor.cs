using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;

namespace Artemis.Core.Services;

/// <summary>
/// Represents a monitor that efficiently keeps track of running processes.
/// </summary>
public static partial class ProcessMonitor
{
    #region Properties & Fields

    private static readonly object LOCK = new();

    private static Timer? _timer;

    private static readonly Dictionary<int, ProcessInfo> _processes = new();

    /// <summary>
    /// Gets an immutable array of the current processes.
    /// </summary>
    public static ImmutableArray<ProcessInfo> Processes
    {
        get
        {
            lock (LOCK)
                return _processes.Values.ToImmutableArray();
        }
    }

    /// <summary>
    /// Gets the date time at which the last update took place.
    /// </summary>
    public static DateTime LastUpdate { get; private set; }

    private static TimeSpan _updateInterval = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets the interval at which to update the list of processes.
    /// </summary>
    public static TimeSpan UpdateInterval
    {
        get => _updateInterval;
        set
        {
            _updateInterval = value;

            lock (LOCK)
                _timer?.Change(TimeSpan.Zero, _updateInterval);
        }
    }

    /// <summary>
    /// Gets a value indicating whether the monitoring has started.
    /// </summary>
    public static bool IsStarted
    {
        get
        {
            lock (LOCK)
                return _timer != null;
        }
    }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when a new process is started.
    /// </summary>
    public static event EventHandler<ProcessEventArgs>? ProcessStarted;

    /// <summary>
    /// Occurs when a process is stopped.
    /// </summary>
    public static event EventHandler<ProcessEventArgs>? ProcessStopped;

    #endregion

    #region Constructors

    static ProcessMonitor()
    {
        Utilities.ShutdownRequested += (_, _) => Stop();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Starts monitoring processes.
    /// </summary>
    public static void Start()
    {
        lock (LOCK)
        {
            if (_timer != null) return;

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                InitializeBuffer();
                _timer = new Timer(UpdateProcessInfosWin32, null, TimeSpan.Zero, UpdateInterval);
            }
            else
            {
                _timer = new Timer(UpdateProcessInfosCrossPlatform, null, TimeSpan.Zero, UpdateInterval);
            }
        }
    }

    /// <summary>
    /// Stops monitoring processes.
    /// </summary>
    public static void Stop()
    {
        lock (LOCK)
        {
            if (_timer == null) return;

            _timer.Dispose();
            _timer = null;

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                FreeBuffer();
        }
    }

    /// <summary>
    ///     Returns whether the specified process is running
    /// </summary>
    /// <param name="processName">The name of the process to check</param>
    /// <param name="processLocation">The location of where the process must be running from (optional)</param>
    /// <returns></returns>
    public static bool IsProcessRunning(string? processName = null, string? processLocation = null)
    {
        if (!IsStarted || (processName == null && processLocation == null))
            return false;

        lock (LOCK)
        {
            return _processes.Values.Any(x => IsProcessRunning(x, processName, processLocation));
        }
    }

    // ReSharper disable once SuggestBaseTypeForParameter
    private static void HandleStoppedProcesses(HashSet<int> currentProcessIds)
    {
        int[] oldProcessIds = _processes.Keys.ToArray();
        foreach (int id in oldProcessIds)
            if (!currentProcessIds.Contains(id))
            {
                ProcessInfo info = _processes[id];
                _processes.Remove(id);
                OnProcessStopped(info);
            }
    }

    private static bool IsProcessRunning(ProcessInfo info, string? processName, string? processLocation)
    {
        if (processName != null && processLocation != null)
            return string.Equals(info.ProcessName, processName, StringComparison.InvariantCultureIgnoreCase) &&
                   string.Equals(Path.GetDirectoryName(info.Executable), processLocation, StringComparison.InvariantCultureIgnoreCase);

        if (processName != null)
            return string.Equals(info.ProcessName, processName, StringComparison.InvariantCultureIgnoreCase);

        if (processLocation != null)
            return string.Equals(Path.GetDirectoryName(info.Executable), processLocation, StringComparison.InvariantCultureIgnoreCase);

        return false;
    }

    private static void OnProcessStarted(ProcessInfo processInfo)
    {
        try
        {
            ProcessStarted?.Invoke(null, new ProcessEventArgs(processInfo));
        }
        catch
        {
            /* Subscribers are idiots! */
        }
    }

    private static void OnProcessStopped(ProcessInfo processInfo)
    {
        try
        {
            ProcessStopped?.Invoke(null, new ProcessEventArgs(processInfo));
        }
        catch
        {
            /* Subscribers are idiots! */
        }
    }

    #endregion
}