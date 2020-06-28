using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Artemis.Core.Events;
using Artemis.Core.Exceptions;
using Artemis.Core.Extensions;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using Artemis.Core.Utilities;
using Artemis.Storage.Entities.Plugins;
using Artemis.Storage.Repositories.Interfaces;
using McMaster.NETCore.Plugins;
using Newtonsoft.Json;
using Ninject;
using Ninject.Extensions.ChildKernel;
using Ninject.Parameters;
using RGB.NET.Core;
using Serilog;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Provides access to plugin loading and unloading
    /// </summary>
    public class PluginService : IPluginService
    {
        private readonly IKernel _kernel;
        private readonly ILogger _logger;
        private readonly IPluginRepository _pluginRepository;
        private readonly List<PluginInfo> _plugins;
        private IKernel _childKernel;

        internal PluginService(IKernel kernel, ILogger logger, IPluginRepository pluginRepository)
        {
            _kernel = kernel;
            _logger = logger;
            _pluginRepository = pluginRepository;
            _plugins = new List<PluginInfo>();

            // Ensure the plugins directory exists
            if (!Directory.Exists(Constants.DataFolder + "plugins"))
                Directory.CreateDirectory(Constants.DataFolder + "plugins");
        }

        /// <inheritdoc />
        public bool LoadingPlugins { get; private set; }

        /// <inheritdoc />
        public void CopyBuiltInPlugins()
        {
            OnCopyingBuildInPlugins();
            var pluginDirectory = new DirectoryInfo(Path.Combine(Constants.DataFolder, "plugins"));

            // Iterate built-in plugins
            var builtInPluginDirectory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "Plugins"));
            foreach (var subDirectory in builtInPluginDirectory.EnumerateDirectories())
            {
                // Load the metadata
                var builtInMetadataFile = Path.Combine(subDirectory.FullName, "plugin.json");
                if (!File.Exists(builtInMetadataFile))
                    throw new ArtemisPluginException("Couldn't find the built-in plugins metadata file at " + builtInMetadataFile);

                var builtInPluginInfo = JsonConvert.DeserializeObject<PluginInfo>(File.ReadAllText(builtInMetadataFile));

                // Find the matching plugin in the plugin folder
                var match = pluginDirectory.EnumerateDirectories().FirstOrDefault(d => d.Name == subDirectory.Name);
                if (match == null)
                    CopyBuiltInPlugin(subDirectory);
                else
                {
                    var metadataFile = Path.Combine(match.FullName, "plugin.json");
                    if (!File.Exists(metadataFile))
                    {
                        _logger.Debug("Copying missing built-in plugin {builtInPluginInfo}", builtInPluginInfo);
                        CopyBuiltInPlugin(subDirectory);
                    }
                    else
                    {
                        try
                        {
                            // Compare versions, copy if the same when debugging
                            var pluginInfo = JsonConvert.DeserializeObject<PluginInfo>(File.ReadAllText(metadataFile));
                            #if DEBUG
                            if (builtInPluginInfo.Version >= pluginInfo.Version)
                            {
                                _logger.Debug("Copying updated built-in plugin {builtInPluginInfo}", builtInPluginInfo);
                                CopyBuiltInPlugin(subDirectory);
                            }
                            #else
                            if (builtInPluginInfo.Version > pluginInfo.Version)
                            {
                                _logger.Debug("Copying updated built-in plugin from {pluginInfo} to {builtInPluginInfo}", pluginInfo, builtInPluginInfo);
                                CopyBuiltInPlugin(subDirectory);
                            }
                            #endif
                        }
                        catch (Exception e)
                        {
                            throw new ArtemisPluginException("Failed read plugin metadata needed to install built-in plugin", e);
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        public void LoadPlugins()
        {
            if (LoadingPlugins)
                throw new ArtemisCoreException("Cannot load plugins while a previous load hasn't been completed yet.");

            lock (_plugins)
            {
                LoadingPlugins = true;

                // Unload all currently loaded plugins first
                UnloadPlugins();

                // Create a child kernel and app domain that will only contain the plugins
                _childKernel = new ChildKernel(_kernel);

                // Load the plugin assemblies into the plugin context
                var pluginDirectory = new DirectoryInfo(Path.Combine(Constants.DataFolder, "plugins"));
                foreach (var subDirectory in pluginDirectory.EnumerateDirectories())
                {
                    try
                    {
                        // Load the metadata
                        var metadataFile = Path.Combine(subDirectory.FullName, "plugin.json");
                        if (!File.Exists(metadataFile)) _logger.Warning(new ArtemisPluginException("Couldn't find the plugins metadata file at " + metadataFile), "Plugin exception");

                        // Locate the main entry
                        var pluginInfo = JsonConvert.DeserializeObject<PluginInfo>(File.ReadAllText(metadataFile));
                        pluginInfo.Directory = subDirectory;

                        LoadPlugin(pluginInfo);
                    }
                    catch (Exception e)
                    {
                        _logger.Warning(new ArtemisPluginException("Failed to load plugin", e), "Plugin exception");
                    }
                }

                // Activate plugins after they are all loaded
                foreach (var pluginInfo in _plugins.Where(p => p.Enabled))
                {
                    try
                    {
                        EnablePlugin(pluginInfo.Instance, true);
                    }
                    catch (Exception)
                    {
                        // ignored, logged in EnablePlugin
                    }
                }

                LoadingPlugins = false;
            }
        }

        /// <inheritdoc />
        public void UnloadPlugins()
        {
            lock (_plugins)
            {
                // Unload all plugins
                while (_plugins.Count > 0)
                    UnloadPlugin(_plugins[0]);

                // Dispose the child kernel and therefore any leftover plugins instantiated with it
                if (_childKernel != null)
                {
                    _childKernel.Dispose();
                    _childKernel = null;
                }

                _plugins.Clear();
            }
        }

        /// <inheritdoc />
        public void LoadPlugin(PluginInfo pluginInfo)
        {
            lock (_plugins)
            {
                _logger.Debug("Loading plugin {pluginInfo}", pluginInfo);
                OnPluginLoading(new PluginEventArgs(pluginInfo));

                // Unload the plugin first if it is already loaded
                if (_plugins.Contains(pluginInfo))
                    UnloadPlugin(pluginInfo);

                var pluginEntity = _pluginRepository.GetPluginByGuid(pluginInfo.Guid);
                if (pluginEntity == null)
                    pluginEntity = new PluginEntity {Id = pluginInfo.Guid, IsEnabled = true};

                pluginInfo.PluginEntity = pluginEntity;
                pluginInfo.Enabled = pluginEntity.IsEnabled;

                var mainFile = Path.Combine(pluginInfo.Directory.FullName, pluginInfo.Main);
                if (!File.Exists(mainFile))
                    throw new ArtemisPluginException(pluginInfo, "Couldn't find the plugins main entry at " + mainFile);

                // Load the plugin, all types implementing Plugin and register them with DI
                pluginInfo.PluginLoader = PluginLoader.CreateFromAssemblyFile(mainFile, configure =>
                {
                    configure.IsUnloadable = true;
                    configure.PreferSharedTypes = true;
                });

                try
                {
                    pluginInfo.Assembly = pluginInfo.PluginLoader.LoadDefaultAssembly();
                }
                catch (Exception e)
                {
                    throw new ArtemisPluginException(pluginInfo, "Failed to load the plugins assembly", e);
                }

                // Get the Plugin implementation from the main assembly and if there is only one, instantiate it
                List<Type> pluginTypes;
                try
                {
                    pluginTypes = pluginInfo.Assembly.GetTypes().Where(t => typeof(Plugin).IsAssignableFrom(t)).ToList();
                }
                catch (ReflectionTypeLoadException e)
                {
                    throw new ArtemisPluginException(pluginInfo, "Failed to initialize the plugin assembly", new AggregateException(e.LoaderExceptions));
                }

                if (pluginTypes.Count > 1)
                    throw new ArtemisPluginException(pluginInfo, $"Plugin contains {pluginTypes.Count} implementations of Plugin, only 1 allowed");
                if (pluginTypes.Count == 0)
                    throw new ArtemisPluginException(pluginInfo, "Plugin contains no implementation of Plugin");

                var pluginType = pluginTypes.Single();
                try
                {
                    var parameters = new IParameter[]
                    {
                        new Parameter("PluginInfo", pluginInfo, false)
                    };
                    pluginInfo.Instance = (Plugin) _childKernel.Get(pluginType, constraint: null, parameters: parameters);
                    pluginInfo.Instance.PluginInfo = pluginInfo;
                }
                catch (Exception e)
                {
                    throw new ArtemisPluginException(pluginInfo, "Failed to instantiate the plugin", e);
                }

                _plugins.Add(pluginInfo);
                OnPluginLoaded(new PluginEventArgs(pluginInfo));
            }
        }

        /// <inheritdoc />
        public void UnloadPlugin(PluginInfo pluginInfo)
        {
            lock (_plugins)
            {
                try
                {
                    pluginInfo.Instance.SetEnabled(false);
                }
                catch (Exception)
                {
                    // TODO: Log these
                }
                finally
                {
                    OnPluginDisabled(new PluginEventArgs(pluginInfo));
                }

                _childKernel.Unbind(pluginInfo.Instance.GetType());

                pluginInfo.Instance.Dispose();
                pluginInfo.PluginLoader.Dispose();
                _plugins.Remove(pluginInfo);

                OnPluginUnloaded(new PluginEventArgs(pluginInfo));
            }
        }

        public void EnablePlugin(Plugin plugin, bool isAutoEnable = false)
        {
            lock (_plugins)
            {
                _logger.Debug("Enabling plugin {pluginInfo}", plugin.PluginInfo);

                try
                {
                    plugin.SetEnabled(true, isAutoEnable);
                }
                catch (Exception e)
                {
                    _logger.Warning(new ArtemisPluginException(plugin.PluginInfo, "Exception during SetEnabled(true)", e), "Failed to enable plugin");
                    throw;
                }
                finally
                {
                    // On an auto-enable, ensure PluginInfo.Enabled is true even if enable failed, that way a failure on auto-enable does
                    // not affect the user's settings
                    if (isAutoEnable) 
                        plugin.PluginInfo.Enabled = true;

                    plugin.PluginInfo.ApplyToEntity();
                    _pluginRepository.SavePlugin(plugin.PluginInfo.PluginEntity);

                    if (plugin.PluginInfo.Enabled) 
                        _logger.Debug("Successfully enabled plugin {pluginInfo}", plugin.PluginInfo);
                }
            }

            OnPluginEnabled(new PluginEventArgs(plugin.PluginInfo));
        }

        public void DisablePlugin(Plugin plugin)
        {
            lock (_plugins)
            {
                _logger.Debug("Disabling plugin {pluginInfo}", plugin.PluginInfo);

                // Device providers cannot be disabled at runtime, restart the application
                if (plugin is DeviceProvider)
                {
                    // Don't call SetEnabled(false) but simply update enabled state and save it
                    plugin.PluginInfo.Enabled = false;
                    plugin.PluginInfo.ApplyToEntity();
                    _pluginRepository.SavePlugin(plugin.PluginInfo.PluginEntity);

                    _logger.Debug("Shutting down for device provider disable {pluginInfo}", plugin.PluginInfo);
                    CurrentProcessUtilities.Shutdown(2, true);
                    return;
                }

                plugin.SetEnabled(false);
                plugin.PluginInfo.ApplyToEntity();
                _pluginRepository.SavePlugin(plugin.PluginInfo.PluginEntity);

                _logger.Debug("Successfully disabled plugin {pluginInfo}", plugin.PluginInfo);
            }

            OnPluginDisabled(new PluginEventArgs(plugin.PluginInfo));
        }

        /// <inheritdoc />
        public PluginInfo GetPluginInfo(Plugin plugin)
        {
            lock (_plugins)
            {
                return _plugins.FirstOrDefault(p => p.Instance == plugin);
            }
        }

        /// <inheritdoc />
        public List<PluginInfo> GetAllPluginInfo()
        {
            return new List<PluginInfo>(_plugins);
        }

        /// <inheritdoc />
        public List<T> GetPluginsOfType<T>() where T : Plugin
        {
            lock (_plugins)
            {
                return _plugins.Where(p => p.Enabled && p.Instance is T).Select(p => (T) p.Instance).ToList();
            }
        }

        public Plugin GetDevicePlugin(IRGBDevice rgbDevice)
        {
            return GetPluginsOfType<DeviceProvider>().First(d => d.RgbDeviceProvider.Devices != null && d.RgbDeviceProvider.Devices.Contains(rgbDevice));
        }

        public void Dispose()
        {
            UnloadPlugins();
        }

        private static void CopyBuiltInPlugin(DirectoryInfo builtInPluginDirectory)
        {
            var pluginDirectory = new DirectoryInfo(Path.Combine(Constants.DataFolder, "plugins", builtInPluginDirectory.Name));
            var createLockFile = File.Exists(Path.Combine(pluginDirectory.FullName, "artemis.lock"));

            // Remove the old directory if it exists
            if (Directory.Exists(pluginDirectory.FullName))
                pluginDirectory.DeleteRecursively();
            Directory.CreateDirectory(pluginDirectory.FullName);

            builtInPluginDirectory.CopyFilesRecursively(pluginDirectory);
            if (createLockFile) 
                File.Create(Path.Combine(pluginDirectory.FullName, "artemis.lock")).Close();
        }

        #region Events

        public event EventHandler CopyingBuildInPlugins;
        public event EventHandler<PluginEventArgs> PluginLoading;
        public event EventHandler<PluginEventArgs> PluginLoaded;
        public event EventHandler<PluginEventArgs> PluginUnloaded;
        public event EventHandler<PluginEventArgs> PluginEnabled;
        public event EventHandler<PluginEventArgs> PluginDisabled;

        protected virtual void OnCopyingBuildInPlugins()
        {
            CopyingBuildInPlugins?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnPluginLoading(PluginEventArgs e)
        {
            PluginLoading?.Invoke(this, e);
        }

        protected virtual void OnPluginLoaded(PluginEventArgs e)
        {
            PluginLoaded?.Invoke(this, e);
        }

        protected virtual void OnPluginUnloaded(PluginEventArgs e)
        {
            PluginUnloaded?.Invoke(this, e);
        }

        protected virtual void OnPluginEnabled(PluginEventArgs e)
        {
            PluginEnabled?.Invoke(this, e);
        }

        protected virtual void OnPluginDisabled(PluginEventArgs e)
        {
            PluginDisabled?.Invoke(this, e);
        }

        #endregion
    }
}