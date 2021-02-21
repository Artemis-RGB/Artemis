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
        }

        private void CopyBuiltInPlugin(FileInfo zipFileInfo, ZipArchive zipArchive)
        {
            DirectoryInfo pluginDirectory = new(Path.Combine(Constants.DataFolder, "plugins", Path.GetFileNameWithoutExtension(zipFileInfo.Name)));
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
            DirectoryInfo pluginDirectory = new(Path.Combine(Constants.DataFolder, "plugins"));

            // Iterate built-in plugins
            DirectoryInfo builtInPluginDirectory = new(Path.Combine(Directory.GetCurrentDirectory(), "Plugins"));
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

                using StreamReader reader = new(metaDataFileEntry.Open());
                PluginInfo builtInPluginInfo = CoreJson.DeserializeObject<PluginInfo>(reader.ReadToEnd())!;

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
                            PluginInfo pluginInfo = CoreJson.DeserializeObject<PluginInfo>(File.ReadAllText(metadataFile))!;

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
            lock (_plugins)
            {
                return new List<Plugin>(_plugins);
            }
        }

        public List<T> GetFeaturesOfType<T>() where T : PluginFeature
        {
            lock (_plugins)
            {
                return _plugins.Where(p => p.IsEnabled).SelectMany(p => p.Features.Where(i => i.IsEnabled && i is T)).Cast<T>().ToList();
            }
        }

        public Plugin? GetPluginByAssembly(Assembly? assembly)
        {
            if (assembly == null)
                return null;
            lock (_plugins)
            {
                return _plugins.FirstOrDefault(p => p.Assembly == assembly);
            }
        }

        // TODO: move to a more appropriate service
        public DeviceProvider GetDeviceProviderByDevice(IRGBDevice rgbDevice)
        {
            return GetFeaturesOfType<DeviceProvider>().First(d => d.RgbDeviceProvider.Devices.Contains(rgbDevice));
        }

        public Plugin? GetCallingPlugin()
        {
            StackTrace stackTrace = new(); // get call stack
            StackFrame[] stackFrames = stackTrace.GetFrames(); // get method calls (frames)

            foreach (StackFrame stackFrame in stackFrames)
            {
                Assembly? assembly = stackFrame.GetMethod()?.DeclaringType?.Assembly;
                Plugin? plugin = GetPluginByAssembly(assembly);
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

        public void LoadPlugins(bool ignorePluginLock, bool stayElevated, bool isElevated)
        {
            if (LoadingPlugins)
                throw new ArtemisCoreException("Cannot load plugins while a previous load hasn't been completed yet.");

            LoadingPlugins = true;

            // Unload all currently loaded plugins first
            UnloadPlugins();

            // Load the plugin assemblies into the plugin context
            DirectoryInfo pluginDirectory = new(Path.Combine(Constants.DataFolder, "plugins"));
            foreach (DirectoryInfo subDirectory in pluginDirectory.EnumerateDirectories())
                try
                {
                    LoadPlugin(subDirectory);
                }
                catch (Exception e)
                {
                    _logger.Warning(new ArtemisPluginException("Failed to load plugin", e), "Plugin exception");
                }

            // ReSharper disable InconsistentlySynchronizedField - It's read-only, idc
            _logger.Debug("Loaded {count} plugin(s)", _plugins.Count);

            bool adminRequired = _plugins.Any(p => p.Entity.IsEnabled && p.Info.RequiresAdmin);
            if (!isElevated && adminRequired)
            {
                _logger.Information("Restarting because one or more plugins requires elevation");
                // No need for a delay this early on, nothing that needs graceful shutdown is happening yet
                Utilities.Restart(true, TimeSpan.Zero);
                return;
            }

            if (isElevated && !adminRequired && !stayElevated)
            {
                // No need for a delay this early on, nothing that needs graceful shutdown is happening yet
                _logger.Information("Restarting because no plugin requires elevation and --force-elevation was not supplied");
                Utilities.Restart(false, TimeSpan.Zero);
                return;
            }

            foreach (Plugin plugin in _plugins.Where(p => p.Entity.IsEnabled))
                EnablePlugin(plugin, false, ignorePluginLock);

            _logger.Debug("Enabled {count} plugin(s)", _plugins.Where(p => p.IsEnabled).Sum(p => p.Features.Count(f => f.IsEnabled)));
            // ReSharper restore InconsistentlySynchronizedField

            LoadingPlugins = false;
        }

        public void UnloadPlugins()
        {
            // Unload all plugins
            // ReSharper disable InconsistentlySynchronizedField - UnloadPlugin will lock it when it has to
            while (_plugins.Count > 0)
                UnloadPlugin(_plugins[0]);
            // ReSharper restore InconsistentlySynchronizedField

            lock (_plugins)
            {
                _plugins.Clear();
            }
        }

        public Plugin LoadPlugin(DirectoryInfo directory)
        {
            _logger.Verbose("Loading plugin from {directory}", directory.FullName);

            // Load the metadata
            string metadataFile = Path.Combine(directory.FullName, "plugin.json");
            if (!File.Exists(metadataFile))
                _logger.Warning(new ArtemisPluginException("Couldn't find the plugins metadata file at " + metadataFile), "Plugin exception");

            // PluginInfo contains the ID which we need to move on
            PluginInfo pluginInfo = CoreJson.DeserializeObject<PluginInfo>(File.ReadAllText(metadataFile))!;

            if (pluginInfo.Guid == Constants.CorePluginInfo.Guid)
                throw new ArtemisPluginException($"Plugin {pluginInfo} cannot use reserved GUID {pluginInfo.Guid}");

            lock (_plugins)
            {
                // Ensure the plugin is not already loaded
                if (_plugins.Any(p => p.Guid == pluginInfo.Guid))
                    throw new ArtemisCoreException($"Cannot load plugin {pluginInfo} because it is using a GUID already used by another plugin");
            }

            // Load the entity and fall back on creating a new one
            Plugin plugin = new(pluginInfo, directory, _pluginRepository.GetPluginByGuid(pluginInfo.Guid));
            OnPluginLoading(new PluginEventArgs(plugin));

            // Locate the main assembly entry
            string? mainFile = plugin.ResolveRelativePath(plugin.Info.Main);
            if (!File.Exists(mainFile))
                throw new ArtemisPluginException(plugin, "Couldn't find the plugins main entry at " + mainFile);
            FileInfo[] fileInfos = directory.GetFiles();
            if (!fileInfos.Any(f => string.Equals(f.Name, plugin.Info.Main, StringComparison.InvariantCulture)))
                throw new ArtemisPluginException(plugin, "Plugin main entry casing mismatch at " + plugin.Info.Main);

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

            lock (_plugins)
            {
                _plugins.Add(plugin);
            }

            OnPluginLoaded(new PluginEventArgs(plugin));
            return plugin;
        }

        public void EnablePlugin(Plugin plugin, bool saveState, bool ignorePluginLock)
        {
            if (plugin.Assembly == null)
                throw new ArtemisPluginException(plugin, "Cannot enable a plugin that hasn't successfully been loaded");

            // Create the Ninject child kernel and load the module
            plugin.Kernel = new ChildKernel(_kernel, new PluginModule(plugin));
            OnPluginEnabling(new PluginEventArgs(plugin));

            plugin.SetEnabled(true);

            // Get the Plugin feature from the main assembly and if there is only one, instantiate it
            List<Type> featureTypes;
            try
            {
                featureTypes = plugin.Assembly.GetTypes().Where(t => typeof(PluginFeature).IsAssignableFrom(t)).ToList();
            }
            catch (ReflectionTypeLoadException e)
            {
                throw new ArtemisPluginException(
                    plugin,
                    "Failed to initialize the plugin assembly",
                    // ReSharper disable once RedundantEnumerableCastCall - Casting from nullable to non-nullable here
                    new AggregateException(e.LoaderExceptions.Where(le => le != null).Cast<Exception>().ToArray())
                );
            }

            if (!featureTypes.Any())
                _logger.Warning("Plugin {plugin} contains no features", plugin);

            // Create instances of each feature and add them to the plugin
            // Construction should be simple and not contain any logic so failure at this point means the entire plugin fails
            foreach (Type featureType in featureTypes)
                try
                {
                    plugin.Kernel.Bind(featureType).ToSelf().InSingletonScope();

                    // Include Plugin as a parameter for the PluginSettingsProvider
                    IParameter[] parameters = {new Parameter("Plugin", plugin, false)};
                    PluginFeature instance = (PluginFeature) plugin.Kernel.Get(featureType, parameters);

                    // Get the PluginFeature attribute which contains extra info on the feature
                    PluginFeatureAttribute? pluginFeatureAttribute = (PluginFeatureAttribute?) Attribute.GetCustomAttribute(featureType, typeof(PluginFeatureAttribute));
                    instance.Info = new PluginFeatureInfo(instance, pluginFeatureAttribute);
                    plugin.AddFeature(instance);

                    // Load the enabled state and if not found, default to true
                    instance.Entity = plugin.Entity.Features.FirstOrDefault(i => i.Type == featureType.FullName) ??
                                      new PluginFeatureEntity {IsEnabled = plugin.Info.AutoEnableFeatures, Type = featureType.FullName!};
                }
                catch (Exception e)
                {
                    _logger.Warning(new ArtemisPluginException(plugin, "Failed to instantiate feature", e), "Failed to instantiate feature", plugin);
                }

            // Activate plugins after they are all loaded
            foreach (PluginFeature pluginFeature in plugin.Features.Where(i => i.Entity.IsEnabled))
                try
                {
                    EnablePluginFeature(pluginFeature, false, !ignorePluginLock);
                }
                catch (Exception)
                {
                    // ignored, logged in EnablePluginFeature
                }

            if (saveState)
            {
                plugin.Entity.IsEnabled = plugin.IsEnabled;
                SavePlugin(plugin);
            }

            OnPluginEnabled(new PluginEventArgs(plugin));
        }

        public void UnloadPlugin(Plugin plugin)
        {
            try
            {
                DisablePlugin(plugin, false);
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
            lock (_plugins)
            {
                _plugins.Remove(plugin);
            }
        }

        public void DisablePlugin(Plugin plugin, bool saveState)
        {
            if (!plugin.IsEnabled)
                return;

            while (plugin.Features.Any())
            {
                PluginFeature feature = plugin.Features[0];
                if (feature.IsEnabled)
                    DisablePluginFeature(feature, false);
                plugin.RemoveFeature(feature);
            }

            plugin.SetEnabled(false);

            plugin.Kernel?.Dispose();
            plugin.Kernel = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            if (saveState)
            {
                plugin.Entity.IsEnabled = plugin.IsEnabled;
                SavePlugin(plugin);
            }

            OnPluginDisabled(new PluginEventArgs(plugin));
        }

        /// <inheritdoc />
        public Plugin ImportPlugin(string fileName)
        {
            DirectoryInfo pluginDirectory = new(Path.Combine(Constants.DataFolder, "plugins"));

            // Find the metadata file in the zip
            using ZipArchive archive = ZipFile.OpenRead(fileName);
            ZipArchiveEntry? metaDataFileEntry = archive.Entries.FirstOrDefault(e => e.Name == "plugin.json");
            if (metaDataFileEntry == null)
                throw new ArtemisPluginException("Couldn't find a plugin.json in " + fileName);


            using StreamReader reader = new(metaDataFileEntry.Open());
            PluginInfo pluginInfo = CoreJson.DeserializeObject<PluginInfo>(reader.ReadToEnd())!;
            if (!pluginInfo.Main.EndsWith(".dll"))
                throw new ArtemisPluginException("Main entry in plugin.json must point to a .dll file" + fileName);

            Plugin? existing = _plugins.FirstOrDefault(p => p.Guid == pluginInfo.Guid);
            if (existing != null)
                throw new ArtemisPluginException($"A plugin with the same GUID is already loaded: {existing.Info}");

            string targetDirectory = pluginInfo.Main.Split(".dll")[0].Replace("/", "").Replace("\\", "");
            string uniqueTargetDirectory = targetDirectory;
            int attempt = 2;

            // Find a unique folder
            while (pluginDirectory.EnumerateDirectories().Any(d => d.Name == uniqueTargetDirectory))
            {
                uniqueTargetDirectory = targetDirectory + "-" + attempt;
                attempt++;
            }

            // Extract everything in the same archive directory to the unique plugin directory
            DirectoryInfo directoryInfo = new(Path.Combine(pluginDirectory.FullName, uniqueTargetDirectory));
            Directory.CreateDirectory(directoryInfo.FullName);
            string metaDataDirectory = metaDataFileEntry.FullName.Replace(metaDataFileEntry.Name, "");
            foreach (ZipArchiveEntry zipArchiveEntry in archive.Entries)
                if (zipArchiveEntry.FullName.StartsWith(metaDataDirectory))
                {
                    string target = Path.Combine(directoryInfo.FullName, zipArchiveEntry.FullName.Remove(0, metaDataDirectory.Length));
                    zipArchiveEntry.ExtractToFile(target);
                }

            // Load the newly extracted plugin and return the result
            return LoadPlugin(directoryInfo);
        }

        #endregion

        #region Features

        public void EnablePluginFeature(PluginFeature pluginFeature, bool saveState, bool isAutoEnable)
        {
            _logger.Verbose("Enabling plugin feature {feature} - {plugin}", pluginFeature, pluginFeature.Plugin);

            OnPluginFeatureEnabling(new PluginFeatureEventArgs(pluginFeature));
            try
            {
                pluginFeature.SetEnabled(true, isAutoEnable);
                if (saveState)
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
                if (saveState)
                {
                    if (isAutoEnable)
                        pluginFeature.Entity.IsEnabled = true;

                    SavePlugin(pluginFeature.Plugin);
                }

                if (pluginFeature.IsEnabled)
                {
                    _logger.Verbose("Successfully enabled plugin feature {feature} - {plugin}", pluginFeature, pluginFeature.Plugin);
                    OnPluginFeatureEnabled(new PluginFeatureEventArgs(pluginFeature));
                }
                else
                {
                    OnPluginFeatureEnableFailed(new PluginFeatureEventArgs(pluginFeature));
                }
            }
        }

        public void DisablePluginFeature(PluginFeature pluginFeature, bool saveState)
        {
            try
            {
                _logger.Verbose("Disabling plugin feature {feature} - {plugin}", pluginFeature, pluginFeature.Plugin);
                pluginFeature.SetEnabled(false);
            }
            finally
            {
                if (saveState)
                {
                    pluginFeature.Entity.IsEnabled = false;
                    SavePlugin(pluginFeature.Plugin);
                }

                if (!pluginFeature.IsEnabled)
                {
                    _logger.Verbose("Successfully disabled plugin feature {feature} - {plugin}", pluginFeature, pluginFeature.Plugin);
                    OnPluginFeatureDisabled(new PluginFeatureEventArgs(pluginFeature));
                }
            }
        }

        #endregion

        #region Storage

        private void SavePlugin(Plugin plugin)
        {
            foreach (PluginFeature pluginFeature in plugin.Features)
                if (plugin.Entity.Features.All(i => i.Type != pluginFeature.GetType().FullName))
                    plugin.Entity.Features.Add(pluginFeature.Entity);

            _pluginRepository.SavePlugin(plugin.Entity);
        }

        #endregion

        #region Events

        public event EventHandler? CopyingBuildInPlugins;
        public event EventHandler<PluginEventArgs>? PluginLoading;
        public event EventHandler<PluginEventArgs>? PluginLoaded;
        public event EventHandler<PluginEventArgs>? PluginUnloaded;
        public event EventHandler<PluginEventArgs>? PluginEnabling;
        public event EventHandler<PluginEventArgs>? PluginEnabled;
        public event EventHandler<PluginEventArgs>? PluginDisabled;

        public event EventHandler<PluginFeatureEventArgs>? PluginFeatureEnabling;
        public event EventHandler<PluginFeatureEventArgs>? PluginFeatureEnabled;
        public event EventHandler<PluginFeatureEventArgs>? PluginFeatureDisabled;
        public event EventHandler<PluginFeatureEventArgs>? PluginFeatureEnableFailed;

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