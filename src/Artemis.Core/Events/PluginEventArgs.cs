using System;
using Artemis.Plugins.Interfaces;

namespace Artemis.Core.Events
{
    public class PluginEventArgs : EventArgs
    {
        public IPlugin Plugin { get; set; }
    }
}