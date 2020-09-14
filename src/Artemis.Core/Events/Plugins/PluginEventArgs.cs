using System;

namespace Artemis.Core
{
    public class PluginEventArgs : EventArgs
    {
        public PluginEventArgs()
        {
        }

        public PluginEventArgs(PluginInfo pluginInfo)
        {
            PluginInfo = pluginInfo;
        }

        public PluginInfo PluginInfo { get; }
    }
}