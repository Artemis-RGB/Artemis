using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using RGB.NET.Core;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     A service providing plugin management
    /// </summary>
    public interface IPluginManagementService : IArtemisService, IDisposable
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
        void LoadPlugins(bool ignorePluginLock);

        /// <summary>
        ///     Unloads all installed plugins.
        /// </summary>
        void UnloadPlugins();

        /// <summary>
        ///     Loads the plugin defined in the provided <see cref="PluginInfo" />
        /// </summary>
        /// <param name="pluginInfo">The plugin info defining the plugin to load</param>
        void LoadPlugin(DirectoryInfo pluginInfo);

        /// <summary>
        ///     Unloads the plugin defined in the provided <see cref="PluginInfo" />
        /// </summary>
        /// <param name="pluginInfo">The plugin info defining the plugin to unload</param>
        void UnloadPlugin(PluginInfo pluginInfo);

        /// <summary>
        ///     Enables the provided plugin
        /// </summary>
        /// <param name="pluginImplementation"></param>
        /// <param name="isAutoEnable">If true, fails if there is a lock file present</param>
        void EnablePluginImplementation(PluginImplementation pluginImplementation, bool isAutoEnable = false);

        /// <summary>
        ///     Disables the provided plugin
        /// </summary>
        /// <param name="pluginImplementation"></param>
        void DisablePluginImplementation(PluginImplementation pluginImplementation);

        /// <summary>
        ///     Finds the plugin info related to the plugin
        /// </summary>
        /// <param name="pluginImplementation">The plugin you want to find the plugin info for</param>
        /// <returns>The plugins PluginInfo</returns>
        PluginInfo GetPluginInfo(PluginImplementation pluginImplementation);

        /// <summary>
        ///     Gets the plugin info of all loaded plugins
        /// </summary>
        /// <returns>A list containing all the plugin info</returns>
        List<PluginInfo> GetAllPluginInfo();

        /// <summary>
        ///     Finds all enabled <see cref="PluginImplementation" /> instances of <typeparamref name="T" />
        /// </summary>
        /// <typeparam name="T">Either <see cref="PluginImplementation" /> or a plugin type implementing <see cref="PluginImplementation" /></typeparam>
        /// <returns>Returns a list of plugin instances of <typeparamref name="T" /></returns>
        List<T> GetPluginsOfType<T>() where T : PluginImplementation;

        /// <summary>
        ///     Gets the plugin that provided the specified assembly
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        PluginImplementation GetPluginByAssembly(Assembly assembly);

        /// <summary>
        ///     Gets the plugin that defined the specified device
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        PluginImplementation GetPluginByDevice(IRGBDevice device);

        /// <summary>
        ///     Returns the plugin info of the current call stack
        /// </summary>
        /// <returns>If the current call stack contains a plugin, the plugin. Otherwise null</returns>
        PluginImplementation GetCallingPlugin();

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
        ///     Occurs when a plugin is being enabled
        /// </summary>
        event EventHandler<PluginEventArgs> PluginEnabling;

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