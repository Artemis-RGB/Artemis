namespace Artemis.Core
{
    /// <summary>
    ///     An optional entry point for your plugin
    /// </summary>
    public interface IPluginBootstrapper
    {
        /// <summary>
        /// Called when the plugin is loaded
        /// </summary>
        /// <param name="plugin"></param>
        void OnPluginLoaded(Plugin plugin);

        /// <summary>
        ///     Called when the plugin is activated
        /// </summary>
        /// <param name="plugin">The plugin instance of your plugin</param>
        void OnPluginEnabled(Plugin plugin);

        /// <summary>
        ///     Called when the plugin is deactivated or when Artemis shuts down
        /// </summary>
        /// <param name="plugin">The plugin instance of your plugin</param>
        void OnPluginDisabled(Plugin plugin);
    }
}