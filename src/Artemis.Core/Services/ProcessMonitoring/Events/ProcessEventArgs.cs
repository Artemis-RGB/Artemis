using System;

namespace Artemis.Core.Services;

/// <summary>
///     Contains data for the ProcessMonitor process events
/// </summary>
public class ProcessEventArgs : EventArgs
{
    #region Properties & Fields

    /// <summary>
    ///     Gets the process info related to the event
    /// </summary>
    public ProcessInfo ProcessInfo { get; }

    #endregion

    #region Constructors

    internal ProcessEventArgs(ProcessInfo processInfo)
    {
        this.ProcessInfo = processInfo;
    }

    #endregion
}