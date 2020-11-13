using System;

namespace Artemis.Core
{
    public class PluginEventArgs : EventArgs
    {
        public PluginEventArgs()
        {
        }

        public PluginEventArgs(Plugin plugin)
        {
            Plugin = plugin;
        }

        public Plugin Plugin { get; }
    }
}