using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AppDomainToolkit;
using Artemis.Core.Events;
using Artemis.Core.Exceptions;
using Artemis.Core.Extensions;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
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
        private readonly List<PluginInfo> _plugins;
        private IKernel _childKernel;

        internal PluginService(IKernel kernel, ILogger logger)
        {
            _kernel = kernel;
            _logger = logger;
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
                    try
                    {
                        pluginInfo.Instance.EnablePlugin();
                    }
                    catch (Exception e)
                    {
                        _logger.Warning(new ArtemisPluginException(pluginInfo, "Failed to load enable plugin", e), "Plugin exception");
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

                // TODO Just temporarily until settings are in place
                pluginInfo.Enabled = true;
                var mainFile = Path.Combine(pluginInfo.Directory.FullName, pluginInfo.Main);
                if (!File.Exists(mainFile))
                    throw new ArtemisPluginException(pluginInfo, "Couldn't find the plugins main entry at " + mainFile);

                // Load the plugin, all types implementing Plugin and register them with DI
                var setupInfo = new AppDomainSetup
                {
                    ApplicationName = pluginInfo.Guid.ToString(),
                    ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                    PrivateBinPath = pluginInfo.Directory.FullName
                };
                pluginInfo.Context = AppDomainContext.Create(setupInfo);

                try
                {
                    pluginInfo.Context.LoadAssemblyWithReferences(LoadMethod.LoadFrom, mainFile);
                }
                catch (Exception e)
                {
                    throw new ArtemisPluginException(pluginInfo, "Failed to load the plugins assembly", e);
                }

                // Get the Plugin implementation from the main assembly and if there is only one, instantiate it
                List<Type> pluginTypes;
                var mainAssembly = pluginInfo.Context.Domain.GetAssemblies().FirstOrDefault(a => a.Location == mainFile);
                if (mainAssembly == null)
                    throw new ArtemisPluginException(pluginInfo, "Found no supported assembly in the plugins main file");
                try
                {
                    pluginTypes = mainAssembly.GetTypes().Where(t => typeof(Plugin).IsAssignableFrom(t)).ToList();
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
                    pluginInfo.Instance.DisablePlugin();
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
                pluginInfo.Context.Dispose();
                _plugins.Remove(pluginInfo);

                OnPluginUnloaded(new PluginEventArgs(pluginInfo));
            }
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
            return GetPluginsOfType<DeviceProvider>().First(d => d.RgbDeviceProvider.Devices.Contains(rgbDevice));
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