namespace Artemis.Core
{
    /// <summary>
    ///     Represents a plugin
    /// </summary>
    public abstract class Plugin
    {
        /// <summary>
        ///     Gets the plugin info related to this plugin
        /// </summary>
        public PluginInfo Info { get; internal set; }

        /// <summary>
        ///     Gets or sets a configuration dialog for this plugin that is accessible in the UI under Settings > Plugins
        /// </summary>
        public PluginConfigurationDialog ConfigurationDialog { get; protected set; }
    }
}