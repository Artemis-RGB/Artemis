using System;
using Artemis.Core;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Represents a view model for a plugin configuration window
    /// </summary>
    public abstract class PluginConfigurationViewModel : ViewModelBase, IPluginConfigurationViewModel
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="PluginConfigurationViewModel" /> class
        /// </summary>
        /// <param name="plugin"></param>
        protected PluginConfigurationViewModel(Plugin plugin)
        {
            Plugin = plugin;
        }

        /// <summary>
        ///     Gets the plugin this configuration view model is associated with
        /// </summary>
        public Plugin Plugin { get; }

        /// <summary>
        ///     Closes the window hosting the view model
        /// </summary>
        public void Close()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        ///     Occurs when the the window hosting the view model should close
        /// </summary>
        public event EventHandler? CloseRequested;
    }
}