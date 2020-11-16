using System;

namespace Artemis.Core
{
    /// <summary>
    ///     An exception thrown when a plugin-related error occurs
    /// </summary>
    public class ArtemisPluginException : Exception
    {
        internal ArtemisPluginException(Plugin plugin)
        {
            Plugin = plugin;
        }

        internal ArtemisPluginException(Plugin plugin, string message) : base(message)
        {
            Plugin = plugin;
        }

        internal ArtemisPluginException(Plugin plugin, string message, Exception inner) : base(message, inner)
        {
            Plugin = plugin;
        }

        internal ArtemisPluginException(string message) : base(message)
        {
        }

        internal ArtemisPluginException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        ///     Gets the plugin the error is related to
        /// </summary>
        public Plugin? Plugin { get; }
    }
}