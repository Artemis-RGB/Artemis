using System;

namespace Artemis.Core;

/// <summary>
///     Provides data about plugin related events
/// </summary>
public class PluginEventArgs : EventArgs
{
    internal PluginEventArgs(Plugin plugin)
    {
        Plugin = plugin;
    }

    /// <summary>
    ///     Gets the plugin this event is related to
    /// </summary>
    public Plugin Plugin { get; }
}