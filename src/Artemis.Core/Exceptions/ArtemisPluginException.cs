using System;

namespace Artemis.Core
{
    public class ArtemisPluginException : Exception
    {
        public ArtemisPluginException(Plugin plugin)
        {
            Plugin = plugin;
        }

        public ArtemisPluginException(Plugin plugin, string message) : base(message)
        {
            Plugin = plugin;
        }

        public ArtemisPluginException(Plugin plugin, string message, Exception inner) : base(message, inner)
        {
            Plugin = plugin;
        }

        public ArtemisPluginException(string message) : base(message)
        {
        }

        public ArtemisPluginException(string message, Exception inner) : base(message, inner)
        {
        }

        public Plugin Plugin { get; }
    }
}