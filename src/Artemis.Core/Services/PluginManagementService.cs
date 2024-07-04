using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core.DeviceProviders;
using Artemis.Core.DryIoc;
using Artemis.Storage.Entities.General;
using Artemis.Storage.Entities.Plugins;
using Artemis.Storage.Entities.Surface;
using Artemis.Storage.Repositories.Interfaces;
using DryIoc;
using McMaster.NETCore.Plugins;
using RGB.NET.Core;
using Serilog;

namespace Artemis.Core.Services;

/// <summary>
///     Provides access to plugin loading and unloading
/// </summary>
internal class PluginManagementService : IPluginManagementService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IContainer _container;
    private readonly ILogger _logger;
    private readonly IPluginRepository _pluginRepository;
    private readonly List<Plugin> _plugins;
    private FileSystemWatcher? _hotReloadWatcher;
    private bool _disposed;
    private bool _isElevated;

    public PluginManagementService(IContainer container, ILogger logger, IPluginRepository pluginRepository, IDeviceRepository deviceRepository)
    {
        _container = container;
        _logger = logger;
        _pluginRepository = pluginRepository;
        _deviceRepository = deviceRepository;
        _plugins = new List<Plugin>();
    }

    public List<DirectoryInfo> AdditionalPluginDirectories { get; } = new();

    public bool LoadingPlugins { get; private set; }


    #region Built in plugins

    public void CopyBuiltInPlugins()
    {
        OnCopyingBuildInPlugins();
        DirectoryInfo pluginDirectory = new(Constants.PluginsFolder);

        if (Directory.Exists(Path.Combine(pluginDirectory.FullName, "Artemis.Plugins.Modules.Overlay-29e3ff97")))
            Directory.Delete(Path.Combine(pluginDirectory.FullName, "Artemis.Plugins.Modules.Overlay-29e3ff97"), true);
        if (Directory.Exists(Path.Combine(pluginDirectory.FullName, "Artemis.Plugins.DataModelExpansions.TestData-ab41d601")))
            Directory.Delete(Path.Combine(pluginDirectory.FullName, "Artemis.Plugins.DataModelExpansions.TestData-ab41d601"), true);

        // Iterate built-in plugins
        DirectoryInfo builtInPluginDirectory = new(Path.Combine(Constants.ApplicationFolder, "Plugins"));
        if (!builtInPluginDirectory.Exists)
        {
            _logger.Warning("No built-in plugins found at {pluginDir}, skipping CopyBuiltInPlugins", builtInPluginDirectory.FullName);
            return;
        }


        foreach (FileInfo zipFile in builtInPluginDirectory.EnumerateFiles("*.zip"))
        {
            try
            {
                ExtractBuiltInPlugin(zipFile, pluginDirectory);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to copy built-in plugin from {ZipFile}", zipFile.FullName);
            }
        }
    }

    private void ExtractBuiltInPlugin(FileInfo zipFile, DirectoryInfo pluginDirectory)
    {
        // Find the metadata file in the zip
        using ZipArchive archive = ZipFile.OpenRead(zipFile.FullName);

        ZipArchiveEntry? metaDataFileEntry = archive.Entries.FirstOrDefault(e => e.Name == "plugin.json");
        if (metaDataFileEntry == null)
            throw new ArtemisPluginException("Couldn't find a plugin.json in " + zipFile.FullName);

        using StreamReader reader = new(metaDataFileEntry.Open());
        PluginInfo builtInPluginInfo = CoreJson.Deserialize<PluginInfo>(reader.ReadToEnd())!;
        string preferred = builtInPluginInfo.PreferredPluginDirectory;

        // Find the matching plugin in the plugin folder
        DirectoryInfo? match = pluginDirectory.EnumerateDirectories().FirstOrDefault(d => d.Name == preferred);
        if (match == null)
        {
            CopyBuiltInPlugin(archive, preferred);
        }
        else
        {
            string metadataFile = Path.Combine(match.FullName, "plugin.json");
            if (!File.Exists(metadataFile))
            {
                _logger.Debug("Copying missing built-in plugin {builtInPluginInfo}", builtInPluginInfo);
                CopyBuiltInPlugin(archive, preferred);
            }
            else if (metaDataFileEntry.LastWriteTime > File.GetLastWriteTime(metadataFile))
            {
                try
                {
                    _logger.Debug("Copying updated built-in plugin {builtInPluginInfo}", builtInPluginInfo);
                    CopyBuiltInPlugin(archive, preferred);
                }
                catch (Exception e)
                {
                    throw new ArtemisPluginException($"Failed to install built-in plugin: {e.Message}", e);
                }
            }
        }
    }

    private void CopyBuiltInPlugin(ZipArchive zipArchive, string targetDirectory)
    {
        ZipArchiveEntry metaDataFileEntry = zipArchive.Entries.First(e => e.Name == "plugin.json");
        DirectoryInfo pluginDirectory = new(Path.Combine(Constants.PluginsFolder, targetDirectory));
        bool createLockFile = File.Exists(Path.Combine(pluginDirectory.FullName, "artemis.lock"));

        // Remove the old directory if it exists
        if (Directory.Exists(pluginDirectory.FullName))
            pluginDirectory.Delete(true);

        // Extract everything in the same archive directory to the unique plugin directory
        Utilities.CreateAccessibleDirectory(pluginDirectory.FullName);
        string metaDataDirectory = metaDataFileEntry.FullName.Replace(metaDataFileEntry.Name, "");
        foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Entries)
        {
            if (zipArchiveEntry.FullName.StartsWith(metaDataDirectory) && !zipArchiveEntry.FullName.EndsWith("/"))
            {
                string target = Path.Combine(pluginDirectory.FullName, zipArchiveEntry.FullName.Remove(0, metaDataDirectory.Length));
                // Create folders
                Utilities.CreateAccessibleDirectory(Path.GetDirectoryName(target)!);
                // Extract files
                zipArchiveEntry.ExtractToFile(target);
            }
        }

        if (createLockFile)
            File.Create(Path.Combine(pluginDirectory.FullName, "artemis.lock")).Close();
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
            return _plugins.Where(p => p.IsEnabled)
                .SelectMany(p => p.Features.Where(f => f.Instance != null && f.Instance.IsEnabled && f.Instance is T))
                .Select(f => f.Instance)
                .Cast<T>()
                .ToList();
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
        return GetPluginFromStackTrace(new StackTrace());
    }

    public Plugin? GetPluginFromException(Exception exception)
    {
        if (exception is ArtemisPluginException pluginException && pluginException.Plugin != null)
            return pluginException.Plugin;

        return GetPluginFromStackTrace(new StackTrace(exception));
    }

    private Plugin? GetPluginFromStackTrace(StackTrace stackTrace)
    {
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

    private Plugin? GetPluginByDirectory(DirectoryInfo directory)
    {
        lock (_plugins)
        {
            return _plugins.FirstOrDefault(p => p.Directory.FullName == directory.FullName);
        }
    }

    public void Dispose()
    {
        // Disposal happens manually before container disposal but the container doesn't know that so a 2nd call will be made
        if (_disposed)
            return;

        _disposed = true;
        UnloadPlugins();
    }

    #region Plugins

    public void LoadPlugins(bool isElevated)
    {
        if (Constants.StartupArguments.Contains("--no-plugins"))
        {
            _logger.Warning("Artemis launched with --no-plugins, skipping the loading of plugins");
            return;
        }

        bool ignorePluginLock = Constants.StartupArguments.Contains("--ignore-plugin-lock");
        bool stayElevated = Constants.StartupArguments.Contains("--force-elevation");
        bool droppedAdmin = Constants.StartupArguments.Contains("--dropped-admin");
        if (LoadingPlugins)
            throw new ArtemisCoreException("Cannot load plugins while a previous load hasn't been completed yet.");

        _isElevated = isElevated;
        LoadingPlugins = true;

        // Unload all currently loaded plugins first
        UnloadPlugins();

        // Load the plugin assemblies into the plugin context
        DirectoryInfo pluginDirectory = new(Constants.PluginsFolder);
        foreach (DirectoryInfo subDirectory in pluginDirectory.EnumerateDirectories())
        {
            try
            {
                LoadPlugin(subDirectory);
            }
            catch (Exception e)
            {
                _logger.Warning(new ArtemisPluginException($"Failed to load plugin at {subDirectory}", e), "Plugin exception");
            }
        }

        foreach (DirectoryInfo directory in AdditionalPluginDirectories)
        {
            try
            {
                LoadPlugin(directory);
            }
            catch (Exception e)
            {
                _logger.Warning(new ArtemisPluginException($"Failed to load plugin at {directory}", e), "Plugin exception");
            }
        }

        // ReSharper disable InconsistentlySynchronizedField - It's read-only, idc
        _logger.Debug("Loaded {count} plugin(s)", _plugins.Count);

        bool adminRequired = _plugins.Any(p => p.Info.RequiresAdmin && p.Entity.IsEnabled && p.HasEnabledFeatures());
        if (!isElevated && adminRequired)
        {
            _logger.Information("Restarting because one or more plugins requires elevation");
            // No need for a delay this early on, nothing that needs graceful shutdown is happening yet
            Utilities.Restart(true, TimeSpan.Zero);
            return;
        }

        if (isElevated && !adminRequired && !stayElevated)
        {
            if (droppedAdmin)
            {
                _logger.Information("No plugin requires elevation but dropping admin failed before, ignoring");
            }
            else
            {
                // No need for a delay this early on, nothing that needs graceful shutdown is happening yet
                _logger.Information("Restarting because no plugin requires elevation and --force-elevation was not supplied");
                Utilities.Restart(false, TimeSpan.Zero, "--dropped-admin");
                return;
            }
        }

        foreach (Plugin plugin in _plugins.Where(p => p.Info.IsCompatible && p.Entity.IsEnabled))
        {
            try
            {
                EnablePlugin(plugin, false, ignorePluginLock);
            }
            catch (ArtemisPluginPrerequisiteException)
            {
                _logger.Warning("Skipped enabling plugin {plugin} because not all prerequisites are met", plugin);
            }
        }

        _logger.Debug("Enabled {count} plugin(s)", _plugins.Count(p => p.IsEnabled));
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

    public Plugin? LoadPlugin(DirectoryInfo directory)
    {
        _logger.Verbose("Loading plugin from {directory}", directory.FullName);

        // Load the metadata
        string metadataFile = Path.Combine(directory.FullName, "plugin.json");
        if (!File.Exists(metadataFile))
            _logger.Warning(new ArtemisPluginException("Couldn't find the plugins metadata file at " + metadataFile), "Plugin exception");

        // PluginInfo contains the ID which we need to move on
        PluginInfo pluginInfo = CoreJson.Deserialize<PluginInfo>(File.ReadAllText(metadataFile))!;

        if (!pluginInfo.IsCompatible)
            return null;

        if (pluginInfo.Guid == Constants.CorePluginInfo.Guid)
            throw new ArtemisPluginException($"Plugin {pluginInfo} cannot use reserved GUID {pluginInfo.Guid}");

        lock (_plugins)
        {
            // Ensure the plugin is not already loaded
            if (_plugins.Any(p => p.Guid == pluginInfo.Guid))
                throw new ArtemisCoreException($"Cannot load plugin {pluginInfo} because it is using a GUID already used by another plugin");
        }

        // Load the entity and fall back on creating a new one
        PluginEntity? entity = _pluginRepository.GetPluginByPluginGuid(pluginInfo.Guid);
        bool loadedFromStorage = entity != null;
        if (entity == null)
        {
            entity = new PluginEntity {PluginGuid = pluginInfo.Guid};
            _pluginRepository.SavePlugin(entity);
        }

        Plugin plugin = new(pluginInfo, directory, entity, loadedFromStorage);
        OnPluginLoading(new PluginEventArgs(plugin));

        // Locate the main assembly entry
        string? mainFile = plugin.ResolveRelativePath(plugin.Info.Main);
        if (!File.Exists(mainFile))
            throw new ArtemisPluginException(plugin, "Couldn't find the plugins main entry at " + mainFile);
        FileInfo[] fileInfos = directory.GetFiles();
        if (!fileInfos.Any(f => string.Equals(f.Name, plugin.Info.Main, StringComparison.InvariantCulture)))
            throw new ArtemisPluginException(plugin, "Plugin main entry casing mismatch at " + plugin.Info.Main);

        // Load the plugin, all types implementing Plugin and register them with DI
        plugin.PluginLoader = PluginLoader.CreateFromAssemblyFile(mainFile, configure =>
        {
            configure.IsUnloadable = true;
            configure.LoadInMemory = true;
            configure.PreferSharedTypes = true;

            // Resolving failed, try a loaded assembly but ignoring the version
            configure.DefaultContext.Resolving += (context, assemblyName) => context.Assemblies.FirstOrDefault(a => a.GetName().Name == assemblyName.Name);
        });

        try
        {
            plugin.Assembly = plugin.PluginLoader.LoadDefaultAssembly();
        }
        catch (Exception e)
        {
            throw new ArtemisPluginException(plugin, "Failed to load the plugins assembly", e);
        }

        // Get the Plugin feature from the main assembly and if there is only one, instantiate it
        List<Type> featureTypes;
        try
        {
            featureTypes = plugin.Assembly.GetTypes().Where(t => typeof(PluginFeature).IsAssignableFrom(t) && !t.IsAbstract).ToList();
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

        bool addedNewFeature = false;
        foreach (Type featureType in featureTypes)
        {
            // Load the enabled state and if not found, default to true
            PluginFeatureEntity? featureEntity = plugin.Entity.Features.FirstOrDefault(i => i.Type == featureType.FullName);
            if (featureEntity == null)
            {
                featureEntity = new PluginFeatureEntity {Type = featureType.FullName!};
                entity.Features.Add(featureEntity);
                addedNewFeature = true;
            }

            PluginFeatureInfo feature = new(plugin, featureType, featureEntity, (PluginFeatureAttribute?) Attribute.GetCustomAttribute(featureType, typeof(PluginFeatureAttribute)));

            // If the plugin only has a single feature, it should always be enabled
            if (featureTypes.Count == 1)
                feature.AlwaysEnabled = true;

            plugin.AddFeature(feature);
        }

        if (!featureTypes.Any())
            _logger.Warning("Plugin {plugin} contains no features", plugin);

        // It is appropriate to call this now that we have the features of this plugin
        bool autoEnabled = plugin.AutoEnableIfNew();

        if (autoEnabled || addedNewFeature)
            _pluginRepository.SavePlugin(entity);

        List<Type> bootstrappers = plugin.Assembly.GetTypes().Where(t => typeof(PluginBootstrapper).IsAssignableFrom(t)).ToList();
        if (bootstrappers.Count > 1)
            _logger.Warning("{Plugin} has more than one bootstrapper, only initializing {FullName}", plugin, bootstrappers.First().FullName);
        if (bootstrappers.Any())
        {
            plugin.Bootstrapper = (PluginBootstrapper?) Activator.CreateInstance(bootstrappers.First());
            plugin.Bootstrapper?.InternalOnPluginLoaded(plugin);
        }

        lock (_plugins)
        {
            _plugins.Add(plugin);
        }

        OnPluginLoaded(new PluginEventArgs(plugin));
        return plugin;
    }

    public void EnablePlugin(Plugin plugin, bool saveState, bool ignorePluginLock)
    {
        if (!plugin.Info.IsCompatible)
            throw new ArtemisPluginException(plugin, $"This plugin only supports the following operating system(s): {plugin.Info.Platforms}");

        if (plugin.Assembly == null)
            throw new ArtemisPluginException(plugin, "Cannot enable a plugin that hasn't successfully been loaded");

        if (plugin.Info.RequiresAdmin && plugin.HasEnabledFeatures() && !_isElevated)
        {
            if (!saveState)
                throw new ArtemisCoreException("Cannot enable a plugin that requires elevation without saving it's state.");

            plugin.Entity.IsEnabled = true;
            SavePlugin(plugin);

            _logger.Information("Restarting because a newly enabled plugin requires elevation");
            Utilities.Restart(true, TimeSpan.FromMilliseconds(500));
            return;
        }

        if (!plugin.Info.ArePrerequisitesMet())
            throw new ArtemisPluginPrerequisiteException(plugin.Info, "Cannot enable a plugin whose prerequisites aren't all met");

        // Create a child container for the plugin, be a bit more forgiving about concrete types
        plugin.Container = _container.CreateChild(newRules: _container.Rules.WithConcreteTypeDynamicRegistrations());
        try
        {
            plugin.Container.RegisterPlugin(plugin);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Failed to register plugin services for plugin {plugin}, skipping enabling", plugin);
            return;
        }

        OnPluginEnabling(new PluginEventArgs(plugin));

        plugin.SetEnabled(true);

        // Create instances of each feature
        // Construction should be simple and not contain any logic so failure at this point means the entire plugin fails
        foreach (PluginFeatureInfo featureInfo in plugin.Features)
        {
            try
            {
                plugin.Container.Register(featureInfo.FeatureType, reuse: Reuse.Singleton);
                PluginFeature instance = (PluginFeature) plugin.Container.Resolve(featureInfo.FeatureType);

                // Get the PluginFeature attribute which contains extra info on the feature
                featureInfo.Instance = instance;
                instance.Info = featureInfo;
                instance.Plugin = plugin;
                instance.Profiler = plugin.GetProfiler("Feature - " + featureInfo.Name);
                instance.Entity = featureInfo.Entity;
            }
            catch (Exception e)
            {
                _logger.Warning(new ArtemisPluginException(plugin, "Failed to instantiate feature", e), "Failed to instantiate feature from plugin {plugin}", plugin);
                featureInfo.LoadException = e;
            }
        }

        // Activate features after they are all loaded
        foreach (PluginFeatureInfo pluginFeature in plugin.Features.Where(f => f.Instance != null && (f.EnabledInStorage || f.AlwaysEnabled)))
        {
            try
            {
                EnablePluginFeature(pluginFeature.Instance!, false, !ignorePluginLock);
            }
            catch (Exception)
            {
                if (pluginFeature.AlwaysEnabled)
                    DisablePlugin(plugin, false);
                throw;
            }
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
            OnPluginUnloaded(new PluginEventArgs(plugin));
        }
    }

    public void DisablePlugin(Plugin plugin, bool saveState)
    {
        if (!plugin.IsEnabled)
            return;

        foreach (PluginFeatureInfo pluginFeatureInfo in plugin.Features)
        {
            if (pluginFeatureInfo.Instance != null && pluginFeatureInfo.Instance.IsEnabled)
                DisablePluginFeature(pluginFeatureInfo.Instance, false);
        }

        plugin.SetEnabled(false);

        plugin.Container?.Dispose();
        plugin.Container = null;

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

    public Plugin? ImportPlugin(string fileName)
    {
        DirectoryInfo pluginDirectory = new(Constants.PluginsFolder);

        // Find the metadata file in the zip
        using ZipArchive archive = ZipFile.OpenRead(fileName);
        ZipArchiveEntry? metaDataFileEntry = archive.Entries.FirstOrDefault(e => e.Name == "plugin.json");
        if (metaDataFileEntry == null)
            throw new ArtemisPluginException("Couldn't find a plugin.json in " + fileName);

        using StreamReader reader = new(metaDataFileEntry.Open());
        PluginInfo pluginInfo = CoreJson.Deserialize<PluginInfo>(reader.ReadToEnd())!;
        if (!pluginInfo.Main.EndsWith(".dll"))
            throw new ArtemisPluginException("Main entry in plugin.json must point to a .dll file");

        Plugin? existing = _plugins.FirstOrDefault(p => p.Guid == pluginInfo.Guid);
        if (existing != null)
            try
            {
                RemovePlugin(existing, false);
            }
            catch (Exception e)
            {
                throw new ArtemisPluginException("A plugin with the same GUID is already loaded, failed to remove old version", e);
            }

        string targetDirectory = pluginInfo.PreferredPluginDirectory;
        if (Directory.Exists(Path.Combine(pluginDirectory.FullName, targetDirectory)))
            Directory.Delete(Path.Combine(pluginDirectory.FullName, targetDirectory), true);

        // Extract everything in the same archive directory to the unique plugin directory
        DirectoryInfo directoryInfo = new(Path.Combine(pluginDirectory.FullName, targetDirectory));
        Utilities.CreateAccessibleDirectory(directoryInfo.FullName);
        string metaDataDirectory = metaDataFileEntry.FullName.Replace(metaDataFileEntry.Name, "");
        foreach (ZipArchiveEntry zipArchiveEntry in archive.Entries)
        {
            if (zipArchiveEntry.FullName.StartsWith(metaDataDirectory) && !zipArchiveEntry.FullName.EndsWith("/"))
            {
                string target = Path.Combine(directoryInfo.FullName, zipArchiveEntry.FullName.Remove(0, metaDataDirectory.Length));
                // Create folders
                Utilities.CreateAccessibleDirectory(Path.GetDirectoryName(target)!);
                // Extract files
                zipArchiveEntry.ExtractToFile(target);
            }
        }

        // Load the newly extracted plugin and return the result
        return LoadPlugin(directoryInfo);
    }

    public void RemovePlugin(Plugin plugin, bool removeSettings)
    {
        DirectoryInfo directory = plugin.Directory;
        lock (_plugins)
        {
            if (_plugins.Contains(plugin))
                UnloadPlugin(plugin);
        }

        // Delete plugin.json since that should never be in use and prevents future loads
        File.Delete(Path.Combine(directory.FullName, "plugin.json"));

        try
        {
            // Give a good effort to remove the directory, files may be in use though :\
            directory.Delete(true);
        }
        catch (Exception e)
        {
            _logger.Warning(e, "Failed to fully remove plugin directory {Directory}", directory.FullName);
        }

        if (removeSettings)
            RemovePluginSettings(plugin);
        
        OnPluginRemoved(new PluginEventArgs(plugin));
    }

    public void RemovePluginSettings(Plugin plugin)
    {
        if (plugin.IsEnabled)
            throw new ArtemisCoreException("Cannot remove the settings of an enabled plugin");

        _pluginRepository.RemoveSettings(plugin.Guid);
        foreach (DeviceEntity deviceEntity in _deviceRepository.GetAll().Where(e => e.DeviceProvider == plugin.Guid.ToString()))
            _deviceRepository.Remove(deviceEntity);

        plugin.Settings?.ClearSettings();
    }

    #endregion

    #region Features

    public void EnablePluginFeature(PluginFeature pluginFeature, bool saveState, bool isAutoEnable)
    {
        _logger.Verbose("Enabling plugin feature {feature} - {plugin}", pluginFeature, pluginFeature.Plugin);

        OnPluginFeatureEnabling(new PluginFeatureEventArgs(pluginFeature));

        if (pluginFeature.Plugin.Info.RequiresAdmin && !_isElevated)
        {
            if (!saveState)
            {
                OnPluginFeatureEnableFailed(new PluginFeatureEventArgs(pluginFeature));
                if (isAutoEnable)
                    _logger.Warning("Skipped auto-enabling plugin feature {feature} - {plugin} because it requires elevation but state isn't being saved", pluginFeature, pluginFeature.Plugin);
                else
                    throw new ArtemisCoreException("Cannot enable a feature that requires elevation without saving it's state.");
            }

            pluginFeature.Entity.IsEnabled = true;
            pluginFeature.Plugin.Entity.IsEnabled = true;
            SavePlugin(pluginFeature.Plugin);

            _logger.Information("Restarting because a newly enabled feature requires elevation");
            Utilities.Restart(true, TimeSpan.FromMilliseconds(500));
            return;
        }

        if (!pluginFeature.Info.ArePrerequisitesMet())
        {
            OnPluginFeatureEnableFailed(new PluginFeatureEventArgs(pluginFeature));
            throw new ArtemisPluginPrerequisiteException(pluginFeature.Info, "Cannot enable a plugin feature whose prerequisites aren't all met");
        }

        try
        {
            pluginFeature.SetEnabled(true, isAutoEnable);
            if (saveState)
                pluginFeature.Entity.IsEnabled = true;
        }
        catch (Exception e)
        {
            if (isAutoEnable)
            {
                // Schedule a retry based on the amount of attempts
                if (pluginFeature.AutoEnableAttempts < 4 && pluginFeature.Plugin.IsEnabled)
                {
                    TimeSpan retryDelay = TimeSpan.FromSeconds(pluginFeature.AutoEnableAttempts * 10);
                    _logger.Warning(
                        e,
                        "Plugin feature '{feature} - {plugin}' failed to enable during attempt ({attempt}/3), scheduling a retry in {retryDelay}.",
                        pluginFeature,
                        pluginFeature.Plugin,
                        pluginFeature.AutoEnableAttempts,
                        retryDelay
                    );

                    Task.Run(async () =>
                    {
                        await Task.Delay(retryDelay);
                        if (!pluginFeature.IsEnabled && !_disposed)
                            EnablePluginFeature(pluginFeature, saveState, true);
                    });
                }
                else
                {
                    _logger.Warning(e, "Plugin feature '{feature} - {plugin}' failed to enable after 3 attempts, giving up.", pluginFeature, pluginFeature.Plugin);
                }
            }
            else
            {
                _logger.Warning(e, "Plugin feature '{feature} - {plugin}' failed to enable.", pluginFeature, pluginFeature.Plugin);
                throw;
            }
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
        foreach (PluginFeatureInfo featureInfo in plugin.Features.Where(i => i.Instance != null))
        {
            if (plugin.Entity.Features.All(i => i.Type != featureInfo.FeatureType.FullName))
                plugin.Entity.Features.Add(featureInfo.Instance!.Entity);
        }

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
    public event EventHandler<PluginEventArgs>? PluginRemoved;

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
    
    protected virtual void OnPluginRemoved(PluginEventArgs e)
    {
        PluginRemoved?.Invoke(this, e);
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

    #region Hot Reload

    public void StartHotReload()
    {
        // Watch for changes in the plugin directory, "plugin.json".
        // If this file is changed, reload the plugin.
        _hotReloadWatcher = new FileSystemWatcher(Constants.PluginsFolder, "plugin.json");
        _hotReloadWatcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName;
        _hotReloadWatcher.Created += FileSystemWatcherOnCreated;
        _hotReloadWatcher.Error += FileSystemWatcherOnError;
        _hotReloadWatcher.IncludeSubdirectories = true;
        _hotReloadWatcher.EnableRaisingEvents = true;
    }

    private void FileSystemWatcherOnError(object sender, ErrorEventArgs e)
    {
        _logger.Error(e.GetException(), "File system watcher error");
    }

    private void FileSystemWatcherOnCreated(object sender, FileSystemEventArgs e)
    {
        string? pluginPath = Path.GetDirectoryName(e.FullPath);
        if (pluginPath == null)
        {
            _logger.Warning("Plugin change detected, but could not get plugin directory. {fullPath}", e.FullPath);
            return;
        }

        DirectoryInfo pluginDirectory = new(pluginPath);
        Plugin? plugin = GetPluginByDirectory(pluginDirectory);

        if (plugin == null)
        {
            _logger.Warning("Plugin change detected, but could not find plugin. {fullPath}", e.FullPath);
            return;
        }

        if (!plugin.Info.HotReloadSupported)
        {
            _logger.Information("Plugin change detected, but hot reload not supported. {pluginName}", plugin.Info.Name);
            return;
        }

        _logger.Information("Plugin change detected, reloading. {pluginName}", plugin.Info.Name);
        bool wasEnabled = plugin.IsEnabled;

        UnloadPlugin(plugin);
        Thread.Sleep(500);
        Plugin? loadedPlugin = LoadPlugin(pluginDirectory);

        if (loadedPlugin == null)
            return;

        if (wasEnabled)
            EnablePlugin(loadedPlugin, true, false);

        _logger.Information("Plugin reloaded. {fullPath}", e.FullPath);
    }

    #endregion
}

/// <summary>
///     Represents a type of plugin management action
/// </summary>
public enum PluginManagementAction
{
    /// <summary>
    ///     A plugin management action that removes a plugin
    /// </summary>
    Delete

    // /// <summary>
    // ///     A plugin management action that updates a plugin
    // /// </summary>
    // Update
}