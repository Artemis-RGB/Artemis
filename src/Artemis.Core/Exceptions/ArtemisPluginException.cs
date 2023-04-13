using System;

namespace Artemis.Core;

/// <summary>
///     An exception thrown when a plugin-related error occurs
/// </summary>
public class ArtemisPluginException : Exception
{
    /// <summary>
    ///     Creates a new instance of the <see cref="ArtemisPluginException" /> class
    /// </summary>
    internal ArtemisPluginException(Plugin plugin)
    {
        Plugin = plugin;
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="ArtemisPluginException" /> class
    /// </summary>
    internal ArtemisPluginException(Plugin plugin, string message) : base(message)
    {
        Plugin = plugin;
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="ArtemisPluginException" /> class
    /// </summary>
    internal ArtemisPluginException(Plugin plugin, string message, Exception inner) : base(message, inner)
    {
        Plugin = plugin;
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="ArtemisPluginException" /> class
    /// </summary>
    public ArtemisPluginException(string message) : base(message)
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="ArtemisPluginException" /> class
    /// </summary>
    public ArtemisPluginException(string message, Exception inner) : base(message, inner)
    {
    }
    
    /// <summary>
    ///     Creates a new instance of the <see cref="ArtemisPluginException" /> class
    /// </summary>
    public ArtemisPluginException(string message, string helpPageId) : base(message)
    {
        HelpPageId = helpPageId;
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="ArtemisPluginException" /> class
    /// </summary>
    public ArtemisPluginException(string message, Exception inner, string helpPageId) : base(message, inner)
    {
        HelpPageId = helpPageId;
    }

    /// <summary>
    ///     Gets the plugin the error is related to
    /// </summary>
    public Plugin? Plugin { get; }
    
    /// <summary>
    ///     Gets the ID of the help page to display for this exception.
    /// </summary>
    public string? HelpPageId { get; }

}