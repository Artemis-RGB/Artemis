using System;

namespace Artemis.Core
{
    public class PluginFeatureEventArgs : EventArgs
    {
        public PluginFeatureEventArgs()
        {
        }

        public PluginFeatureEventArgs(PluginFeature pluginFeature)
        {
            PluginFeature = pluginFeature;
        }

        public PluginFeature PluginFeature { get; }
    }
}