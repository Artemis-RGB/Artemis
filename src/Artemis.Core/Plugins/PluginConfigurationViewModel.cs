using Stylet;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a view model for a plugin configuration window
    /// </summary>
    public abstract class PluginConfigurationViewModel : Screen
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
        ///     Creates a new instance of the <see cref="PluginConfigurationViewModel" /> class with a validator
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="validator"></param>
        protected PluginConfigurationViewModel(Plugin plugin, IModelValidator validator) : base(validator)
        {
            Plugin = plugin;
        }

        /// <summary>
        ///     Gets the plugin this configuration view model is associated with
        /// </summary>
        public Plugin Plugin { get; }
    }
}