namespace Artemis.Plugins.Interfaces
{
    /// <summary>
    ///     This is the base plugin type, use the other interfaces such as IModule to create plugins
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        ///     Called when the plugin is loaded
        /// </summary>
        void LoadPlugin();

        /// <summary>
        ///     Called when the plugin is unloaded
        /// </summary>
        void UnloadPlugin();
    }
}