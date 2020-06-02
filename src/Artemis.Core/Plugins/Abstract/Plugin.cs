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
        ///     Gets whether the plugin is enabled
        /// </summary>
        public bool Enabled { get; private set; }

        /// <summary>
        ///     Gets or sets whether this plugin has a configuration view model.
        ///     If set to true, <see cref="GetConfigurationViewModel" /> will be called when the plugin is configured from the UI.
        /// </summary>
        public bool HasConfigurationViewModel { get; protected set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Called when the plugin is activated
        /// </summary>
        protected abstract void EnablePlugin();

        /// <summary>
        ///     Called when the plugin is deactivated
        /// </summary>
        protected abstract void DisablePlugin();

        /// <summary>
        ///     Called when the plugins configuration window is opened from the UI. The UI will only attempt to open if
        ///     <see cref="HasConfigurationViewModel" /> is set to True.
        /// </summary>
        /// <returns></returns>
        public virtual PluginConfigurationViewModel GetConfigurationViewModel()
        {
            return null;
        }

        /// <summary>
        ///     Called when Artemis shuts down
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        internal void SetEnabled(bool enable)
        {
            if (enable && !Enabled)
            {
                EnablePlugin();
                OnPluginEnabled();
            }
            else if (!enable && Enabled)
            {
                DisablePlugin();
                OnPluginDisabled();
            }

            Enabled = enable;
        }

        #region Events

        public event EventHandler PluginEnabled;
        public event EventHandler PluginDisabled;

        protected virtual void OnPluginEnabled()
        {
            PluginEnabled?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnPluginDisabled()
        {
            PluginDisabled?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}