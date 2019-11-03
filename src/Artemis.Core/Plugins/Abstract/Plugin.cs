using System;
using System.Threading.Tasks;
using Artemis.Core.Plugins.Models;

namespace Artemis.Core.Plugins.Abstract
{
    /// <inheritdoc />
    /// <summary>
    ///     This is the base plugin type, use the other interfaces such as Module to create plugins
    /// </summary>
    public abstract class Plugin : IDisposable
    {
        internal Plugin(PluginInfo pluginInfo)
        {
            PluginInfo = pluginInfo ?? throw new ArgumentNullException(nameof(pluginInfo));
        }

        public PluginInfo PluginInfo { get; internal set; }

        /// <summary>
        ///     Called when the plugin is activated
        /// </summary>
        public abstract void EnablePlugin();

        /// <summary>
        ///     Called when the plugin is deactivated
        /// </summary>
        public abstract void DisablePlugin();

        /// <inheritdoc />
        /// <summary>
        ///     Called when the plugin is unloaded, clean up any unmanaged resources here
        /// </summary>
        public abstract void Dispose();
    }
}