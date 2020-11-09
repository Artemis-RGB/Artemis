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
        /// <param name="pluginImplementation"></param>
        protected PluginConfigurationViewModel(PluginImplementation pluginImplementation)
        {
            PluginImplementation = pluginImplementation;
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="PluginConfigurationViewModel" /> class with a validator
        /// </summary>
        /// <param name="pluginImplementation"></param>
        /// <param name="validator"></param>
        protected PluginConfigurationViewModel(PluginImplementation pluginImplementation, IModelValidator validator) : base(validator)
        {
            PluginImplementation = pluginImplementation;
        }

        /// <summary>
        ///     Gets the plugin this configuration view model is associated with
        /// </summary>
        public PluginImplementation PluginImplementation { get; }
    }
}