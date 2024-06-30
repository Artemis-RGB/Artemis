using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Artemis.Core.DeviceProviders;
using RGB.NET.Core;

namespace Artemis.Core.Services;

/// <summary>
///     A service providing plugin management
/// </summary>
public interface IPluginManagementService : IArtemisService, IDisposable
{
    /// <summary>
    ///     Gets a list containing additional directories in which plugins are located, used while loading plugins.
    /// </summary>
    List<DirectoryInfo> AdditionalPluginDirectories { get; }

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
    void LoadPlugins(bool isElevated);

    /// <summary>
    ///    Starts monitoring plugin directories for changes and reloads plugins when changes are detected
    /// </summary>
    void StartHotReload();

    /// <summary>
    ///     Unloads all installed plugins.
    /// </summary>
    void UnloadPlugins();

    /// <summary>
    ///     Loads the plugin located in the provided <paramref name="directory" />
    /// </summary>
    /// <param name="directory">The directory where the plugin is located</param>
    Plugin? LoadPlugin(DirectoryInfo directory);

    /// <summary>
    ///     Enables the provided <paramref name="plugin" />
    /// </summary>
    /// <param name="plugin">The plugin to enable</param>
    /// <param name="saveState">Whether or not to save the new enabled state</param>
    /// <param name="ignorePluginLock">
    ///     Whether or not plugin lock files should be ignored. If set to <see langword="true" />,
    ///     plugins with lock files will load successfully
    /// </param>
    void EnablePlugin(Plugin plugin, bool saveState, bool ignorePluginLock = false);

    /// <summary>
    ///     Unloads the provided <paramref name="plugin" />
    /// </summary>
    /// <param name="plugin">The plugin to unload</param>
    void UnloadPlugin(Plugin plugin);

    /// <summary>
    ///     Disables the provided <paramref name="plugin" />
    /// </summary>
    /// <param name="plugin">The plugin to disable</param>
    /// <param name="saveState">Whether or not to save the new enabled state</param>
    void DisablePlugin(Plugin plugin, bool saveState);

    /// <summary>
    ///     Imports the plugin contained in the provided ZIP file
    /// </summary>
    /// <param name="fileName">The full path to the ZIP file that contains the plugin</param>
    /// <returns>The resulting plugin</returns>
    Plugin? ImportPlugin(string fileName);

    /// <summary>
    ///     Unloads and permanently removes the provided plugin
    /// </summary>
    /// <param name="plugin">The plugin to remove</param>
    /// <param name="removeSettings"></param>
    void RemovePlugin(Plugin plugin, bool removeSettings);

    /// <summary>
    ///     Removes the settings of a disabled plugin
    /// </summary>
    /// <param name="plugin">The plugin whose settings to remove</param>
    void RemovePluginSettings(Plugin plugin);

    /// <summary>
    ///     Enables the provided plugin feature
    /// </summary>
    /// <param name="pluginFeature">The feature to enable</param>
    /// <param name="saveState">Whether or not to save the new enabled state</param>
    /// <param name="isAutoEnable">If true, fails if there is a lock file present</param>
    void EnablePluginFeature(PluginFeature pluginFeature, bool saveState, bool isAutoEnable = false);

    /// <summary>
    ///     Disables the provided plugin feature
    /// </summary>
    /// <param name="pluginFeature">The feature to enable</param>
    /// <param name="saveState">Whether or not to save the new enabled state</param>
    void DisablePluginFeature(PluginFeature pluginFeature, bool saveState);

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
    Plugin? GetPluginByAssembly(Assembly? assembly);

    /// <summary>
    ///     Returns the plugin info of the current call stack
    /// </summary>
    /// <returns>If the current call stack contains a plugin, the plugin. Otherwise null</returns>
    Plugin? GetCallingPlugin();

    /// <summary>
    ///     Returns the plugin that threw the provided exception.
    /// </summary>
    /// <param name="exception"></param>
    /// <returns>If the exception was thrown by a plugin, the plugin. Otherwise null</returns>
    Plugin? GetPluginFromException(Exception exception);

    /// <summary>
    ///     Gets the plugin that defined the specified device
    /// </summary>
    /// <param name="device"></param>
    /// <returns></returns>
    DeviceProvider GetDeviceProviderByDevice(IRGBDevice device);

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
    ///     Occurs when a plugin is removed
    /// </summary>
    event EventHandler<PluginEventArgs> PluginRemoved; 

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
}