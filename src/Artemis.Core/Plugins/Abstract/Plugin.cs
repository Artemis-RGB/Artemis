using System;
using Artemis.Core.Plugins.Abstract.ViewModels;
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
        ///     Gets or sets whether this plugin has a configuration view model.
        ///     If set to true, <see cref="GetConfigurationViewModel" /> will be called when the plugin is configured from the UI.
        /// </summary>
        public bool HasConfigurationViewModel { get; protected set; }

        /// <inheritdoc />
        /// <summary>
        ///     Called when the plugin is unloaded, clean up any unmanaged resources here
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        ///     Called when the plugin is activated
        /// </summary>
        public abstract void EnablePlugin();

        /// <summary>
        ///     Called when the plugin is deactivated
        /// </summary>
        public abstract void DisablePlugin();

        /// <summary>
        ///     Called when the plugin's configuration window is opened from the UI. The UI will only attempt to open if
        ///     <see cref="HasConfigurationViewModel" /> is set to True.
        /// </summary>
        /// <returns></returns>
        public virtual PluginConfigurationViewModel GetConfigurationViewModel()
        {
            return null;
        }
    }
}