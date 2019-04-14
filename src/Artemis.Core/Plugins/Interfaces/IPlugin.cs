using System;

namespace Artemis.Core.Plugins.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    ///     This is the base plugin type, use the other interfaces such as IModule to create plugins
    /// </summary>
    public interface IPlugin : IDisposable
    {
        /// <summary>
        ///     Called when the plugin is activated
        /// </summary>
        void EnablePlugin();


        /// <summary>
        ///     Called when the plugin is deactivated
        /// </summary>
        void DisablePlugin();
    }
}