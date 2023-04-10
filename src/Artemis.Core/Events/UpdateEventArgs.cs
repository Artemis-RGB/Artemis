using System;

namespace Artemis.Core;

/// <summary>
///     Provides data about application update events
/// </summary>
public class UpdateEventArgs : EventArgs
{
    internal UpdateEventArgs(bool silent)
    {
        Silent = silent;
    }

    /// <summary>
    ///     Gets a boolean indicating whether to silently update or not.
    /// </summary>
    public bool Silent { get; }
}