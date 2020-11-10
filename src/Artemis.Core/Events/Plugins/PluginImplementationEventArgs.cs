using System;

namespace Artemis.Core
{
    public class PluginImplementationEventArgs : EventArgs
    {
        public PluginImplementationEventArgs()
        {
        }

        public PluginImplementationEventArgs(PluginImplementation pluginImplementation)
        {
            PluginImplementation = pluginImplementation;
        }

        public PluginImplementation PluginImplementation { get; }
    }
}