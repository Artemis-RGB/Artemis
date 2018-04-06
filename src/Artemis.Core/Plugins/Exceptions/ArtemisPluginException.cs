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

        public PluginInfo PluginInfo { get; }
    }
}