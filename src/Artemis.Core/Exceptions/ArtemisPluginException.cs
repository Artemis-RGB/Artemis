using System;
using Artemis.Plugins.Models;

namespace Artemis.Core.Exceptions
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