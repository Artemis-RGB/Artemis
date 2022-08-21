using System;
using System.Diagnostics;

namespace Artemis.Core.Services;

/// <summary>
///     Contains data for the ProcessMonitor process events
/// </summary>
public class ProcessEventArgs : EventArgs
{
    internal ProcessEventArgs(Process process)
    {
        Process = process;
    }

    /// <summary>
    ///     Gets the process related to the event
    /// </summary>
    public Process Process { get; }
}