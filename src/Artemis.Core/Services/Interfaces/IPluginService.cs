using System;
using System.Collections.Generic;
using Artemis.Core.Events;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Models;
using RGB.NET.Core;

namespace Artemis.Core.Services.Interfaces
{
    public interface IPluginService : IArtemisService, IDisposable
    {
        /// <summary>
        ///     Indicates whether or not plugins are currently being loaded
        /// </summary>
        bool LoadingPlugins { get; }

        /// <summary>
        ///     Copy built-in plugins from the executable directory to the plugins directory if the version is higher
        ///     (higher or equal if compiled as debug)
        /// </summary>
        void CopyBuiltInPlugins();

        /// <summary>
        ///     Loads all installed plugins. If plugins already loaded this will reload them all
        /// </summary>
        void LoadPlugins();

        /// <summary>
        ///     Unloads all installed plugins.
        /// </summary>
        void UnloadPlugins();

        /// <summary>
        ///     Loads the plugin defined in the provided <see cref="PluginInfo" />
        /// </summary>
        /// <param name="pluginInfo">The plugin info defining the plugin to load</param>
        void LoadPlugin(PluginInfo pluginInfo);

        /// <summary>
        ///     Unloads the plugin defined in the provided <see cref="PluginInfo" />
        /// </summary>
        /// <param name="pluginInfo">The plugin info defining the plugin to unload</param>
        void UnloadPlugin(PluginInfo pluginInfo);

        /// <summary>
        ///     Finds the plugin info related to the plugin
        /// </summary>
        /// <param name="plugin">The plugin you want to find the plugin info for</param>
        /// <returns>The plugins PluginInfo</returns>
        PluginInfo GetPluginInfo(Plugin plugin);

        /// <summary>
        ///     Gets the plugin info of all loaded plugins
        /// </summary>
        /// <returns>A list containing all the plugin info</returns>
        List<PluginInfo> GetAllPluginInfo();

        /// <summary>
        ///     Finds all enabled <see cref="Plugin" /> instances of type <see cref="T" />
        /// </summary>
        /// <typeparam name="T">Either <see cref="Plugin" /> or a plugin type implementing <see cref="Plugin" /></typeparam>
        /// <returns>Returns a list of plug instances of type <see cref="T" /></returns>
        List<T> GetPluginsOfType<T>() where T : Plugin;

        Plugin GetDevicePlugin(IRGBDevice device);

        #region Events

        /// <summary>
        ///     Occurs when built-in plugins are being loaded
        /// </summary>
        event EventHandler CopyingBuildInPlugins;

        /// <summary>
        ///     Occurs when a plugin has started loading
        /// </summary>
        event EventHandler<PluginEventArgs> PluginLoading;

        /// <summary>
        ///     Occurs when a plugin has loaded
        /// </summary>
        event EventHandler<PluginEventArgs> PluginLoaded;

        /// <summary>
        ///     Occurs when a plugin has been unloaded
        /// </summary>
        event EventHandler<PluginEventArgs> PluginUnloaded;

        /// <summary>
        ///     Occurs when a plugin has been enabled
        /// </summary>
        event EventHandler<PluginEventArgs> PluginEnabled;

        /// <summary>
        ///     Occurs when a plugin has been disabled
        /// </summary>
        event EventHandler<PluginEventArgs> PluginDisabled;

        #endregion
    }
}