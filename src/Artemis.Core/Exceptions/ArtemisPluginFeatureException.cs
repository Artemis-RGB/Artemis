using System;

namespace Artemis.Core
{
    public class ArtemisPluginFeatureException : Exception
    {
        public PluginFeature PluginFeature { get; }

        public ArtemisPluginFeatureException(PluginFeature pluginFeature)
        {
            PluginFeature = pluginFeature;
        }

        public ArtemisPluginFeatureException(PluginFeature pluginFeature, string message) : base(message)
        {
            PluginFeature = pluginFeature;
        }

        public ArtemisPluginFeatureException(PluginFeature pluginFeature, string message, Exception inner) : base(message, inner)
        {
            PluginFeature = pluginFeature;
        }
    }
}