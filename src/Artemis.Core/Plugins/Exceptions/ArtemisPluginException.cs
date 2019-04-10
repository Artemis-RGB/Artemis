using System;
using Artemis.Core.Plugins.Models;

namespace Artemis.Core.Plugins.Exceptions
{
    public class ArtemisPluginException : Exception
    {
        public ArtemisPluginException(PluginInfo pluginInfo)
        {
            PluginInfo = pluginInfo;
        }

        public ArtemisPluginException(PluginInfo pluginInfo, string message) : base(message)
        {
            PluginInfo = pluginInfo;
        }

        public ArtemisPluginException(PluginInfo pluginInfo, string message, Exception inner) : base(message, inner)
        {
            PluginInfo = pluginInfo;
        }

        public ArtemisPluginException(string message) : base(message)
        {
        }

        public ArtemisPluginException(string message, Exception inner) : base(message, inner)
        {
        }

        public PluginInfo PluginInfo { get; }
    }
}