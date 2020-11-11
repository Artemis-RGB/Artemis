using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Artemis.Core.DeviceProviders;
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
        ///     Loads the plugin located in the provided <paramref name="directory" />
        /// </summary>
        /// <param name="directory">The directory where the plugin is located</param>
        Plugin LoadPlugin(DirectoryInfo directory);

        /// <summary>
        ///     Enables the provided <paramref name="plugin" />
        /// </summary>
        /// <param name="plugin">The plugin to enable</param>
        void EnablePlugin(Plugin plugin, bool ignorePluginLock = false);

        /// <summary>
        ///     Unloads the provided <paramref name="plugin" />
        /// </summary>
        /// <param name="plugin">The plugin to unload</param>
        void UnloadPlugin(Plugin plugin);

        /// <summary>
        ///     Disables the provided <paramref name="plugin" />
        /// </summary>
        /// <param name="plugin">The plugin to disable</param>
        void DisablePlugin(Plugin plugin);

        /// <summary>
        ///     Enables the provided plugin feature
        /// </summary>
        /// <param name="pluginFeature"></param>
        /// <param name="isAutoEnable">If true, fails if there is a lock file present</param>
        void EnablePluginFeature(PluginFeature pluginFeature, bool isAutoEnable = false);

        /// <summary>
        ///     Disables the provided plugin feature
        /// </summary>
        /// <param name="pluginFeature"></param>
        void DisablePluginFeature(PluginFeature pluginFeature);

        /// <summary>
        ///     Gets the plugin info of all loaded plugins
        /// </summary>
        /// <returns>A list containing all the plugin info</returns>
        List<Plugin> GetAllPlugins();

        /// <summary>
        ///     Finds all enabled <see cref="PluginFeature" /> instances of <typeparamref name="T" />
        /// </summary>
        /// <typeparam name="T">
        ///     Either <see cref="PluginFeature" /> or a plugin type implementing
        ///     <see cref="PluginFeature" />
        /// </typeparam>
        /// <returns>Returns a list of feature instances of <typeparamref name="T" /></returns>
        List<T> GetFeaturesOfType<T>() where T : PluginFeature;

        /// <summary>
        ///     Gets the plugin that provided the specified assembly
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        Plugin GetPluginByAssembly(Assembly assembly);

        /// <summary>
        ///     Returns the plugin info of the current call stack
        /// </summary>
        /// <returns>If the current call stack contains a plugin, the plugin. Otherwise null</returns>
        Plugin? GetCallingPlugin();

        /// <summary>
        ///     Gets the plugin that defined the specified device
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        DeviceProvider GetDeviceProviderByDevice(IRGBDevice device);

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

        /// <summary>
        ///     Occurs when a plugin feature is being enabled
        /// </summary>
        public event EventHandler<PluginFeatureEventArgs> PluginFeatureEnabling;

        /// <summary>
        ///     Occurs when a plugin feature has been enabled
        /// </summary>
        public event EventHandler<PluginFeatureEventArgs> PluginFeatureEnabled;

        /// <summary>
        ///     Occurs when a plugin feature could not be enabled
        /// </summary>
        public event EventHandler<PluginFeatureEventArgs> PluginFeatureEnableFailed;

        /// <summary>
        ///     Occurs when a plugin feature has been disabled
        /// </summary>
        public event EventHandler<PluginFeatureEventArgs> PluginFeatureDisabled;

        #endregion
    }
}