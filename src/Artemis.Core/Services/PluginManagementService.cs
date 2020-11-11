using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Artemis.Core.DeviceProviders;
using Artemis.Core.Ninject;
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
    internal class PluginManagementService : IPluginManagementService
    {
        private readonly IKernel _kernel;
        private readonly ILogger _logger;
        private readonly IPluginRepository _pluginRepository;
        private readonly List<Plugin> _plugins;

        public PluginManagementService(IKernel kernel, ILogger logger, IPluginRepository pluginRepository)
        {
            _kernel = kernel;
            _logger = logger;
            _pluginRepository = pluginRepository;
            _plugins = new List<Plugin>();

            // Ensure the plugins directory exists
            if (!Directory.Exists(Constants.DataFolder + "plugins"))
                Directory.CreateDirectory(Constants.DataFolder + "plugins");
        }

        private void CopyBuiltInPlugin(FileInfo zipFileInfo, ZipArchive zipArchive)
        {
            DirectoryInfo pluginDirectory = new DirectoryInfo(Path.Combine(Constants.DataFolder, "plugins", Path.GetFileNameWithoutExtension(zipFileInfo.Name)));
            bool createLockFile = File.Exists(Path.Combine(pluginDirectory.FullName, "artemis.lock"));

            // Remove the old directory if it exists
            if (Directory.Exists(pluginDirectory.FullName))
                pluginDirectory.DeleteRecursively();
            Directory.CreateDirectory(pluginDirectory.FullName);

            zipArchive.ExtractToDirectory(pluginDirectory.FullName, true);
            if (createLockFile)
                File.Create(Path.Combine(pluginDirectory.FullName, "artemis.lock")).Close();
        }

        public bool LoadingPlugins { get; private set; }

        #region Built in plugins

        public void CopyBuiltInPlugins()
        {
            OnCopyingBuildInPlugins();
            DirectoryInfo pluginDirectory = new DirectoryInfo(Path.Combine(Constants.DataFolder, "plugins"));

            // Iterate built-in plugins
            DirectoryInfo builtInPluginDirectory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "Plugins"));
            if (!builtInPluginDirectory.Exists)
            {
                _logger.Warning("No built-in plugins found at {pluginDir}, skipping CopyBuiltInPlugins", builtInPluginDirectory.FullName);
                return;
            }

            foreach (FileInfo zipFile in builtInPluginDirectory.EnumerateFiles("*.zip"))
            {
                // Find the metadata file in the zip
                using ZipArchive archive = ZipFile.OpenRead(zipFile.FullName);
                ZipArchiveEntry? metaDataFileEntry = archive.GetEntry("plugin.json");
                if (metaDataFileEntry == null)
                    throw new ArtemisPluginException("Couldn't find a plugin.json in " + zipFile.FullName);

                using StreamReader reader = new StreamReader(metaDataFileEntry.Open());
                PluginInfo builtInPluginInfo = JsonConvert.DeserializeObject<PluginInfo>(reader.ReadToEnd());

                // Find the matching plugin in the plugin folder
                DirectoryInfo? match = pluginDirectory.EnumerateDirectories().FirstOrDefault(d => d.Name == Path.GetFileNameWithoutExtension(zipFile.Name));
                if (match == null)
                {
                    CopyBuiltInPlugin(zipFile, archive);
                }
                else
                {
                    string metadataFile = Path.Combine(match.FullName, "plugin.json");
                    if (!File.Exists(metadataFile))
                    {
                        _logger.Debug("Copying missing built-in plugin {builtInPluginInfo}", builtInPluginInfo);
                        CopyBuiltInPlugin(zipFile, archive);
                    }
                    else
                    {
                        try
                        {
                            // Compare versions, copy if the same when debugging
                            PluginInfo pluginInfo = JsonConvert.DeserializeObject<PluginInfo>(File.ReadAllText(metadataFile));

                            if (builtInPluginInfo.Version > pluginInfo.Version)
                            {
                                _logger.Debug("Copying updated built-in plugin from {pluginInfo} to {builtInPluginInfo}", pluginInfo, builtInPluginInfo);
                                CopyBuiltInPlugin(zipFile, archive);
                            }
                        }
                        catch (Exception e)
                        {
                            throw new ArtemisPluginException("Failed read plugin metadata needed to install built-in plugin", e);
                        }
                    }
                }
            }
        }

        #endregion

        public List<Plugin> GetAllPlugins()
        {
            return new List<Plugin>(_plugins);
        }

        public List<T> GetFeaturesOfType<T>() where T : PluginFeature
        {
            return _plugins.Where(p => p.IsEnabled).SelectMany(p => p.Features.Where(i => i.IsEnabled && i is T)).Cast<T>().ToList();
        }

        public Plugin? GetPluginByAssembly(Assembly assembly)
        {
            return _plugins.FirstOrDefault(p => p.Assembly == assembly);
        }

        // TODO: move to a more appropriate service
        public DeviceProvider GetDeviceProviderByDevice(IRGBDevice rgbDevice)
        {
            return GetFeaturesOfType<DeviceProvider>().First(d => d.RgbDeviceProvider.Devices != null && d.RgbDeviceProvider.Devices.Contains(rgbDevice));
        }

        public Plugin? GetCallingPlugin()
        {
            StackTrace stackTrace = new StackTrace(); // get call stack
            StackFrame[] stackFrames = stackTrace.GetFrames(); // get method calls (frames)

            foreach (StackFrame stackFrame in stackFrames)
            {
                Assembly assembly = stackFrame.GetMethod().DeclaringType.Assembly;
                Plugin plugin = GetPluginByAssembly(assembly);
                if (plugin != null)
                    return plugin;
            }

            return null;
        }

        public void Dispose()
        {
            UnloadPlugins();
        }

        #region Plugins

        public void LoadPlugins(bool ignorePluginLock)
        {
            if (LoadingPlugins)
                throw new ArtemisCoreException("Cannot load plugins while a previous load hasn't been completed yet.");

            lock (_plugins)
            {
                LoadingPlugins = true;

                // Unload all currently loaded plugins first
                UnloadPlugins();

                // Load the plugin assemblies into the plugin context
                DirectoryInfo pluginDirectory = new DirectoryInfo(Path.Combine(Constants.DataFolder, "plugins"));
                foreach (DirectoryInfo subDirectory in pluginDirectory.EnumerateDirectories())
                {
                    try
                    {
                        Plugin plugin = LoadPlugin(subDirectory);
                        EnablePlugin(plugin, ignorePluginLock);
                    }
                    catch (Exception e)
                    {
                        _logger.Warning(new ArtemisPluginException("Failed to load plugin", e), "Plugin exception");
                    }
                }

                LoadingPlugins = false;
            }
        }

        public void UnloadPlugins()
        {
            lock (_plugins)
            {
                // Unload all plugins
                while (_plugins.Count > 0)
                    UnloadPlugin(_plugins[0]);

                _plugins.Clear();
            }
        }

        public Plugin LoadPlugin(DirectoryInfo directory)
        {
            lock (_plugins)
            {
                _logger.Debug("Loading plugin from {directory}", directory.FullName);

                // Load the metadata
                string metadataFile = Path.Combine(directory.FullName, "plugin.json");
                if (!File.Exists(metadataFile))
                    _logger.Warning(new ArtemisPluginException("Couldn't find the plugins metadata file at " + metadataFile), "Plugin exception");

                // PluginInfo contains the ID which we need to move on
                PluginInfo pluginInfo = JsonConvert.DeserializeObject<PluginInfo>(File.ReadAllText(metadataFile));

                if (pluginInfo.Guid == Constants.CorePluginInfo.Guid)
                    throw new ArtemisPluginException($"Plugin cannot use reserved GUID {pluginInfo.Guid}");

                // Ensure the plugin is not already loaded
                if (_plugins.Any(p => p.Guid == pluginInfo.Guid))
                    throw new ArtemisCoreException("Cannot load a plugin that is already loaded");

                Plugin plugin = new Plugin(pluginInfo, directory);
                OnPluginLoading(new PluginEventArgs(plugin));

                // Load the entity and fall back on creating a new one
                plugin.Entity = _pluginRepository.GetPluginByGuid(pluginInfo.Guid) ?? new PluginEntity {Id = plugin.Guid, IsEnabled = true};

                // Locate the main assembly entry
                string? mainFile = plugin.ResolveRelativePath(plugin.Info.Main);
                if (!File.Exists(mainFile))
                    throw new ArtemisPluginException(plugin, "Couldn't find the plugins main entry at " + mainFile);

                // Load the plugin, all types implementing Plugin and register them with DI
                plugin.PluginLoader = PluginLoader.CreateFromAssemblyFile(mainFile!, configure =>
                {
                    configure.IsUnloadable = true;
                    configure.PreferSharedTypes = true;
                });

                try
                {
                    plugin.Assembly = plugin.PluginLoader.LoadDefaultAssembly();
                }
                catch (Exception e)
                {
                    throw new ArtemisPluginException(plugin, "Failed to load the plugins assembly", e);
                }

                List<Type> bootstrappers = plugin.Assembly.GetTypes().Where(t => typeof(IPluginBootstrapper).IsAssignableFrom(t)).ToList();
                if (bootstrappers.Count > 1)
                    _logger.Warning($"{plugin} has more than one bootstrapper, only initializing {bootstrappers.First().FullName}");
                if (bootstrappers.Any())
                    plugin.Bootstrapper = (IPluginBootstrapper?) Activator.CreateInstance(bootstrappers.First());

                _plugins.Add(plugin);

                OnPluginLoaded(new PluginEventArgs(plugin));

                return plugin;
            }
        }

        public void EnablePlugin(Plugin plugin, bool ignorePluginLock)
        {
            if (plugin.Assembly == null)
                throw new ArtemisPluginException(plugin, "Cannot enable a plugin that hasn't successfully been loaded");

            // Create the Ninject child kernel and load the module
            plugin.Kernel = new ChildKernel(_kernel);
            plugin.Kernel.Bind<Plugin>().ToConstant(plugin);
            plugin.Kernel.Load(new PluginModule(plugin.Info));

            plugin.SetEnabled(true);

            // Get the Plugin feature from the main assembly and if there is only one, instantiate it
            List<Type> featureTypes;
            try
            {
                featureTypes = plugin.Assembly.GetTypes().Where(t => typeof(PluginFeature).IsAssignableFrom(t)).ToList();
            }
            catch (ReflectionTypeLoadException e)
            {
                throw new ArtemisPluginException(plugin, "Failed to initialize the plugin assembly", new AggregateException(e.LoaderExceptions));
            }

            if (!featureTypes.Any())
                _logger.Warning("Plugin {plugin} contains no features", plugin);

            // Create instances of each feature and add them to the plugin
            // Construction should be simple and not contain any logic so failure at this point means the entire plugin fails
            foreach (Type featureType in featureTypes)
            {
                try
                {
                    // Include Plugin as a parameter for the PluginSettingsProvider
                    IParameter[] parameters = {new Parameter("Plugin", plugin, false)};
                    PluginFeature instance = (PluginFeature) plugin.Kernel.Get(featureType, parameters);
                    plugin.AddFeature(instance);

                    // Load the enabled state and if not found, default to true
                    instance.Entity = plugin.Entity.Features.FirstOrDefault(i => i.Type == featureType.FullName) ??
                                      new PluginFeatureEntity {IsEnabled = true, Type = featureType.FullName};
                }
                catch (Exception e)
                {
                    throw new ArtemisPluginException(plugin, "Failed to load instantiate feature", e);
                }
            }

            // Activate plugins after they are all loaded
            foreach (PluginFeature pluginFeature in plugin.Features.Where(i => i.Entity.IsEnabled))
            {
                try
                {
                    EnablePluginFeature(pluginFeature, !ignorePluginLock);
                }
                catch (Exception)
                {
                    // ignored, logged in EnablePluginFeature
                }
            }

            OnPluginEnabled(new PluginEventArgs(plugin));
        }

        public void UnloadPlugin(Plugin plugin)
        {
            lock (_plugins)
            {
                try
                {
                    DisablePlugin(plugin);
                }
                catch (Exception e)
                {
                    _logger.Warning(new ArtemisPluginException(plugin, "Exception during DisablePlugin call for UnloadPlugin", e), "Failed to unload plugin");
                }
                finally
                {
                    OnPluginDisabled(new PluginEventArgs(plugin));
                }

                plugin.Dispose();
                _plugins.Remove(plugin);
            }
        }

        public void DisablePlugin(Plugin plugin)
        {
            if (!plugin.IsEnabled)
                return;

            while (plugin.Features.Any())
            {
                PluginFeature feature = plugin.Features[0];
                plugin.RemoveFeature(feature);
                OnPluginFeatureDisabled(new PluginFeatureEventArgs(feature));
            }

            plugin.SetEnabled(false);

            plugin.Kernel.Dispose();
            plugin.Kernel = null;

            OnPluginDisabled(new PluginEventArgs(plugin));
        }

        #endregion

        #region Features

        public void EnablePluginFeature(PluginFeature pluginFeature, bool isAutoEnable = false)
        {
            _logger.Debug("Enabling plugin feature {feature} - {plugin}", pluginFeature, pluginFeature.Plugin);

            lock (_plugins)
            {
                OnPluginFeatureEnabling(new PluginFeatureEventArgs(pluginFeature));
                try
                {
                    // A device provider may be queued for disable on next restart, this undoes that
                    if (pluginFeature is DeviceProvider && pluginFeature.IsEnabled && !pluginFeature.Entity.IsEnabled)
                    {
                        pluginFeature.Entity.IsEnabled = true;
                        SavePlugin(pluginFeature.Plugin);
                        return;
                    }

                    pluginFeature.SetEnabled(true, isAutoEnable);
                    pluginFeature.Entity.IsEnabled = true;
                }
                catch (Exception e)
                {
                    _logger.Warning(
                        new ArtemisPluginException(pluginFeature.Plugin, $"Exception during SetEnabled(true) on {pluginFeature}", e),
                        "Failed to enable plugin"
                    );
                    throw;
                }
                finally
                {
                    // On an auto-enable, ensure PluginInfo.Enabled is true even if enable failed, that way a failure on auto-enable does
                    // not affect the user's settings
                    if (isAutoEnable)
                        pluginFeature.Entity.IsEnabled = true;

                    SavePlugin(pluginFeature.Plugin);

                    if (pluginFeature.IsEnabled)
                    {
                        _logger.Debug("Successfully enabled plugin feature {feature} - {plugin}", pluginFeature, pluginFeature.Plugin);
                        OnPluginFeatureEnabled(new PluginFeatureEventArgs(pluginFeature));
                    }
                    else
                    {
                        OnPluginFeatureEnableFailed(new PluginFeatureEventArgs(pluginFeature));
                    }
                }
            }
        }

        public void DisablePluginFeature(PluginFeature pluginFeature)
        {
            lock (_plugins)
            {
                try
                {
                    _logger.Debug("Disabling plugin feature {feature} - {plugin}", pluginFeature, pluginFeature.Plugin);

                    // Device providers cannot be disabled at runtime simply queue a disable for next restart
                    if (pluginFeature is DeviceProvider)
                    {
                        // Don't call SetEnabled(false) but simply update enabled state and save it
                        pluginFeature.Entity.IsEnabled = false;
                        SavePlugin(pluginFeature.Plugin);
                        return;
                    }

                    pluginFeature.SetEnabled(false);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                finally
                {
                    pluginFeature.Entity.IsEnabled = false;
                    SavePlugin(pluginFeature.Plugin);

                    if (!pluginFeature.IsEnabled)
                    {
                        _logger.Debug("Successfully disabled plugin feature {feature} - {plugin}", pluginFeature, pluginFeature.Plugin);
                        OnPluginFeatureDisabled(new PluginFeatureEventArgs(pluginFeature));
                    }
                }
            }
        }

        #endregion

        #region Storage

        private void SavePlugin(Plugin plugin)
        {
            foreach (PluginFeature pluginFeature in plugin.Features)
            {
                if (plugin.Entity.Features.All(i => i.Type != pluginFeature.GetType().FullName))
                    plugin.Entity.Features.Add(pluginFeature.Entity);
            }

            _pluginRepository.SavePlugin(plugin.Entity);
        }

        private PluginFeatureEntity GetOrCreateFeatureEntity(PluginFeature feature)
        {
            return feature.Plugin.Entity.Features.FirstOrDefault(i => i.Type == feature.GetType().FullName) ??
                   new PluginFeatureEntity {IsEnabled = true, Type = feature.GetType().FullName};
        }

        #endregion

        #region Events

        public event EventHandler CopyingBuildInPlugins;
        public event EventHandler<PluginEventArgs> PluginLoading;
        public event EventHandler<PluginEventArgs> PluginLoaded;
        public event EventHandler<PluginEventArgs> PluginUnloaded;
        public event EventHandler<PluginEventArgs> PluginEnabling;
        public event EventHandler<PluginEventArgs> PluginEnabled;
        public event EventHandler<PluginEventArgs> PluginDisabled;

        public event EventHandler<PluginFeatureEventArgs> PluginFeatureEnabling;
        public event EventHandler<PluginFeatureEventArgs> PluginFeatureEnabled;
        public event EventHandler<PluginFeatureEventArgs> PluginFeatureDisabled;
        public event EventHandler<PluginFeatureEventArgs> PluginFeatureEnableFailed;

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

        protected virtual void OnPluginEnabling(PluginEventArgs e)
        {
            PluginEnabling?.Invoke(this, e);
        }

        protected virtual void OnPluginEnabled(PluginEventArgs e)
        {
            PluginEnabled?.Invoke(this, e);
        }

        protected virtual void OnPluginDisabled(PluginEventArgs e)
        {
            PluginDisabled?.Invoke(this, e);
        }

        protected virtual void OnPluginFeatureEnabling(PluginFeatureEventArgs e)
        {
            PluginFeatureEnabling?.Invoke(this, e);
        }

        protected virtual void OnPluginFeatureEnabled(PluginFeatureEventArgs e)
        {
            PluginFeatureEnabled?.Invoke(this, e);
        }

        protected virtual void OnPluginFeatureDisabled(PluginFeatureEventArgs e)
        {
            PluginFeatureDisabled?.Invoke(this, e);
        }

        protected virtual void OnPluginFeatureEnableFailed(PluginFeatureEventArgs e)
        {
            PluginFeatureEnableFailed?.Invoke(this, e);
        }

        #endregion
    }
}