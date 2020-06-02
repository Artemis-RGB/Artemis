using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
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
                        _logger.Information("Copying missing built-in plugin {name} version: {version}",
                            builtInPluginInfo.Name, builtInPluginInfo.Version);
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
                                _logger.Information("Copying updated built-in plugin {name} version: {version} (old version: {oldVersion})",
                                    builtInPluginInfo.Name, builtInPluginInfo.Version, pluginInfo.Version);
                                CopyBuiltInPlugin(subDirectory);
                            }
                            #else
                            if (builtInPluginInfo.Version > pluginInfo.Version) 
                            {
                                _logger.Information("Copying updated built-in plugin {name} version: {version} (old version: {oldVersion})", 
                                    builtInPluginInfo.Name, builtInPluginInfo.Version, pluginInfo.Version);
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
                        OnPluginLoading(new PluginEventArgs(pluginInfo));
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
                    if (!pluginInfo.PluginEntity.LastEnableSuccessful)
                    {
                        pluginInfo.Enabled = false;
                        _logger.Warning("Plugin failed to load last time, disabling it now to avoid instability. Plugin info: {pluginInfo}", pluginInfo);
                        continue;
                    }

                    // Mark this as false until the plugin enabled successfully and save it in case the plugin drags us down into a crash
                    pluginInfo.PluginEntity.LastEnableSuccessful = false;
                    _pluginRepository.SavePlugin(pluginInfo.PluginEntity);

                    var threwException = false;
                    try
                    {
                        pluginInfo.Instance.SetEnabled(true);
                    }
                    catch (Exception e)
                    {
                        _logger.Warning(new ArtemisPluginException(pluginInfo, "Failed to load enable plugin", e), "Plugin exception");
                        pluginInfo.Enabled = false;
                        threwException = true;
                    }

                    // We got this far so the plugin enabled and we didn't crash horribly, yay
                    if (!threwException)
                    {
                        pluginInfo.PluginEntity.LastEnableSuccessful = true;
                        _pluginRepository.SavePlugin(pluginInfo.PluginEntity);
                    }

                    OnPluginEnabled(new PluginEventArgs(pluginInfo));
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
                // Unload the plugin first if it is already loaded
                if (_plugins.Contains(pluginInfo))
                    UnloadPlugin(pluginInfo);

                var pluginEntity = _pluginRepository.GetPluginByGuid(pluginInfo.Guid);
                if (pluginEntity == null)
                    pluginEntity = new PluginEntity {PluginGuid = pluginInfo.Guid, IsEnabled = true, LastEnableSuccessful = true};

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
                        new ConstructorArgument("pluginInfo", pluginInfo),
                        new Parameter("PluginInfo", pluginInfo, false)
                    };
                    pluginInfo.Instance = (Plugin) _childKernel.Get(pluginType, constraint: null, parameters: parameters);
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

        public void EnablePlugin(Plugin plugin)
        {
            plugin.PluginInfo.Enabled = true;
            plugin.PluginInfo.PluginEntity.IsEnabled = true;
            plugin.PluginInfo.PluginEntity.LastEnableSuccessful = false;
            _pluginRepository.SavePlugin(plugin.PluginInfo.PluginEntity);

            var threwException = false;
            try
            {
                plugin.SetEnabled(true);
            }
            catch (Exception e)
            {
                _logger.Warning(new ArtemisPluginException(plugin.PluginInfo, "Failed to enable plugin", e), "Plugin exception");
                plugin.PluginInfo.Enabled = false;
                threwException = true;
            }

            // We got this far so the plugin enabled and we didn't crash horribly, yay
            if (!threwException)
            {
                plugin.PluginInfo.PluginEntity.LastEnableSuccessful = true;
                _pluginRepository.SavePlugin(plugin.PluginInfo.PluginEntity);
            }

            OnPluginEnabled(new PluginEventArgs(plugin.PluginInfo));
        }

        public void DisablePlugin(Plugin plugin)
        {
            plugin.PluginInfo.Enabled = false;
            plugin.PluginInfo.PluginEntity.IsEnabled = false;
            _pluginRepository.SavePlugin(plugin.PluginInfo.PluginEntity);

            // Device providers cannot be disabled at runtime, restart the application
            if (plugin is DeviceProvider)
            {
                CurrentProcessUtilities.Shutdown(2, true);
                return;
            }

            plugin.SetEnabled(false);
            
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

            // Remove the old directory if it exists
            if (Directory.Exists(pluginDirectory.FullName))
                pluginDirectory.RecursiveDelete();
            Directory.CreateDirectory(pluginDirectory.FullName);

            builtInPluginDirectory.CopyFilesRecursively(pluginDirectory);
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