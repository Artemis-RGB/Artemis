using System.Reactive;
using Artemis.Core;
using ReactiveUI;

namespace Artemis.UI.Avalonia.Shared
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
            Close = ReactiveCommand.Create(() => { });
        }

        /// <summary>
        ///     Gets the plugin this configuration view model is associated with
        /// </summary>
        public Plugin Plugin { get; }

        /// <summary>
        ///     A command that closes the window
        /// </summary>
        public ReactiveCommand<Unit, Unit> Close { get; }
    }
}