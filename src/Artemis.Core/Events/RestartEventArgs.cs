using System;
using System.Collections.Generic;

namespace Artemis.Core;

/// <summary>
///     Provides data about application restart events
/// </summary>
public class RestartEventArgs : EventArgs
{
    internal RestartEventArgs(bool elevate, TimeSpan delay, List<string>? extraArgs)
    {
        Elevate = elevate;
        Delay = delay;
        ExtraArgs = extraArgs;
    }

    /// <summary>
    ///     Gets a boolean indicating whether the application should be restarted with elevated permissions
    /// </summary>
    public bool Elevate { get; }

    /// <summary>
    ///     Gets the delay before killing process and restarting
    /// </summary>
    public TimeSpan Delay { get; }

    /// <summary>
    ///     A list of extra arguments to pass to Artemis when restarting
    /// </summary>
    public List<string>? ExtraArgs { get; }
}