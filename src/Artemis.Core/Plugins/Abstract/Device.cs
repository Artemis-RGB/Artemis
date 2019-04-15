using Artemis.Core.Plugins.Models;

namespace Artemis.Core.Plugins.Abstract
{
    /// <inheritdoc />
    /// <summary>
    ///     Allows you to implement your own RGB device
    /// </summary>
    public abstract class Device : Plugin
    {
        protected Device(PluginInfo pluginInfo) : base(pluginInfo)
        {
        }
    }
}