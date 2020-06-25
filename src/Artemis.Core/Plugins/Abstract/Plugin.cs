using System;
using Artemis.Core.Plugins.Abstract.ViewModels;
using Artemis.Core.Plugins.Models;

namespace Artemis.Core.Plugins.Abstract
{
    /// <summary>
    ///     This is the base plugin type, use the other interfaces such as Module to create plugins
    /// </summary>
    public abstract class Plugin : IDisposable
    {
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
            DisablePlugin();
        }

        /// <summary>
        ///     Called when the plugin is activated
        /// </summary>
        public abstract void EnablePlugin();

        /// <summary>
        ///     Called when the plugin is deactivated or when Artemis shuts down
        /// </summary>
        public abstract void DisablePlugin();

        /// <summary>
        ///     Called when the plugins configuration window is opened from the UI. The UI will only attempt to open if
        ///     <see cref="HasConfigurationViewModel" /> is set to True.
        /// </summary>
        /// <returns></returns>
        public virtual PluginConfigurationViewModel GetConfigurationViewModel()
        {
            return null;
        }

        internal void SetEnabled(bool enable)
        {
            if (enable && !Enabled)
            {
                Enabled = true;
                PluginInfo.Enabled = true;
                
                // If enable failed, put it back in a disabled state
                try
                {
                    EnablePlugin();
                }
                catch
                {
                    Enabled = false;
                    PluginInfo.Enabled = false;
                    throw;
                }

                OnPluginEnabled();
            }
            else if (!enable && Enabled)
            {
                Enabled = false;
                PluginInfo.Enabled = false;

                // Even if disable failed, still leave it in a disabled state to avoid more issues
                DisablePlugin();
                
                OnPluginDisabled();
            }
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