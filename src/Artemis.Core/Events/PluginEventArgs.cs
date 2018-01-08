using System;
using Artemis.Plugins.Interfaces;

namespace Artemis.Core.Events
{
    public class PluginEventArgs : EventArgs
    {
        public PluginEventArgs()
        {
        }

        public PluginEventArgs(IPlugin plugin)
        {
            Plugin = plugin;
        }

        public IPlugin Plugin { get; }
    }
}