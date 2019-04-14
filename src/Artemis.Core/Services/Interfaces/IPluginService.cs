using System;
using System.Collections.Generic;
using Artemis.Core.Events;
using Artemis.Core.Plugins.Interfaces;
using Artemis.Core.Plugins.Models;

namespace Artemis.Core.Services.Interfaces
{
    public interface IPluginService : IArtemisService, IDisposable
    {
        /// <summary>
        ///     Indicates whether or not plugins are currently being loaded
        /// </summary>
        bool LoadingPlugins { get; }

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
        PluginInfo GetPluginInfo(IPlugin plugin);

        /// <summary>
        ///     Gets the plugin info of all loaded plugins
        /// </summary>
        /// <returns>A list containing all the plugin info</returns>
        List<PluginInfo> GetAllPluginInfo();

        /// <summary>
        ///     Finds an instance of the layer type matching the given GUID
        /// </summary>
        /// <param name="layerTypeGuid">The GUID of the layer type to find</param>
        /// <returns>An instance of the layer type</returns>
        ILayerType GetLayerTypeByGuid(Guid layerTypeGuid);

        /// <summary>
        ///     Finds all enabled <see cref="IPlugin" /> instances of type <see cref="T" />
        /// </summary>
        /// <typeparam name="T">Either <see cref="IPlugin" /> or a plugin type implementing <see cref="IPlugin" /></typeparam>
        /// <returns>Returns a list of plug instances of type <see cref="T" /></returns>
        List<T> GetPluginsOfType<T>() where T : IPlugin;

        #region Events

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