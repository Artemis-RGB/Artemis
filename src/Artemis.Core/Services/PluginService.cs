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
    internal class PluginService : IPluginService
    {
        private readonly IKernel _kernel;
        private readonly ILogger _logger;
        private readonly IPluginRepository _pluginRepository;
        private readonly List<PluginInfo> _plugins;

        public PluginService(IKernel kernel, ILogger logger, IPluginRepository pluginRepository)
        {
            _kernel = kernel;
            _logger = logger;
            _pluginRepository = pluginRepository;
            _plugins = new List<PluginInfo>();

            // Ensure the plugins directory exists
            if (!Directory.Exists(Constants.DataFolder + "plugins"))
                Directory.CreateDirectory(Constants.DataFolder + "plugins");
        }

        public bool LoadingPlugins { get; private set; }

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
                    CopyBuiltInPlugin(zipFile, archive);
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
                        // Load the metadata
                        string metadataFile = Path.Combine(subDirectory.FullName, "plugin.json");
                        if (!File.Exists(metadataFile)) _logger.Warning(new ArtemisPluginException("Couldn't find the plugins metadata file at " + metadataFile), "Plugin exception");

                        // Locate the main entry
                        PluginInfo pluginInfo = JsonConvert.DeserializeObject<PluginInfo>(File.ReadAllText(metadataFile));
                        pluginInfo.Directory = subDirectory;

                        LoadPlugin(pluginInfo);
                    }
                    catch (Exception e)
                    {
                        _logger.Warning(new ArtemisPluginException("Failed to load plugin", e), "Plugin exception");
                    }
                }

                // Activate plugins after they are all loaded
                foreach (PluginInfo pluginInfo in _plugins.Where(p => p.IsEnabled))
                {
                    try
                    {
                        EnablePlugin(pluginInfo.Instance, !ignorePluginLock);
                    }
                    catch (Exception)
                    {
                        // ignored, logged in EnablePlugin
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

        public void LoadPlugin(PluginInfo pluginInfo)
        {
            lock (_plugins)
            {
                _logger.Debug("Loading plugin {pluginInfo}", pluginInfo);
                OnPluginLoading(new PluginEventArgs(pluginInfo));

                // Unload the plugin first if it is already loaded
                if (_plugins.Contains(pluginInfo))
                    UnloadPlugin(pluginInfo);

                PluginEntity pluginEntity = _pluginRepository.GetPluginByGuid(pluginInfo.Guid);
                if (pluginEntity == null)
                    pluginEntity = new PluginEntity {Id = pluginInfo.Guid, IsEnabled = true};

                pluginInfo.PluginEntity = pluginEntity;
                pluginInfo.IsEnabled = pluginEntity.IsEnabled;

                string mainFile = Path.Combine(pluginInfo.Directory.FullName, pluginInfo.Main);
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
                    pluginTypes = pluginInfo.Assembly.GetTypes().Where(t => typeof(PluginImplementation).IsAssignableFrom(t)).ToList();
                }
                catch (ReflectionTypeLoadException e)
                {
                    throw new ArtemisPluginException(pluginInfo, "Failed to initialize the plugin assembly", new AggregateException(e.LoaderExceptions));
                }

                if (pluginTypes.Count > 1)
                    throw new ArtemisPluginException(pluginInfo, $"Plugin contains {pluginTypes.Count} implementations of Plugin, only 1 allowed");
                if (pluginTypes.Count == 0)
                    throw new ArtemisPluginException(pluginInfo, "Plugin contains no implementation of Plugin");

                Type pluginType = pluginTypes.Single();
                try
                {
                    IParameter[] parameters = {
                        new Parameter("PluginInfo", pluginInfo, false)
                    };
                    pluginInfo.Kernel = new ChildKernel(_kernel);
                    pluginInfo.Kernel.Load(new PluginModule(pluginInfo));
                    pluginInfo.Instance = (PluginImplementation) pluginInfo.Kernel.Get(pluginType, constraint: null, parameters: parameters);
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

                pluginInfo.Instance.Dispose();
                pluginInfo.PluginLoader.Dispose();
                pluginInfo.Kernel.Dispose();

                _plugins.Remove(pluginInfo);

                OnPluginUnloaded(new PluginEventArgs(pluginInfo));
            }
        }

        public void EnablePlugin(PluginImplementation pluginImplementation, bool isAutoEnable = false)
        {
            _logger.Debug("Enabling plugin {pluginInfo}", pluginImplementation.PluginInfo);
            OnPluginEnabling(new PluginEventArgs(pluginImplementation.PluginInfo));

            lock (_plugins)
            {
                try
                {
                    // A device provider may be queued for disable on next restart, this undoes that
                    if (pluginImplementation is DeviceProvider && pluginImplementation.IsEnabled && !pluginImplementation.PluginInfo.IsEnabled)
                    {
                        pluginImplementation.PluginInfo.IsEnabled = true;
                        pluginImplementation.PluginInfo.ApplyToEntity();
                        _pluginRepository.SavePlugin(pluginImplementation.PluginInfo.PluginEntity);
                        return;
                    }

                    pluginImplementation.SetEnabled(true, isAutoEnable);
                }
                catch (Exception e)
                {
                    _logger.Warning(new ArtemisPluginException(pluginImplementation.PluginInfo, "Exception during SetEnabled(true)", e), "Failed to enable plugin");
                    throw;
                }
                finally
                {
                    // On an auto-enable, ensure PluginInfo.Enabled is true even if enable failed, that way a failure on auto-enable does
                    // not affect the user's settings
                    if (isAutoEnable)
                        pluginImplementation.PluginInfo.IsEnabled = true;

                    pluginImplementation.PluginInfo.ApplyToEntity();
                    _pluginRepository.SavePlugin(pluginImplementation.PluginInfo.PluginEntity);

                    if (pluginImplementation.PluginInfo.IsEnabled)
                        _logger.Debug("Successfully enabled plugin {pluginInfo}", pluginImplementation.PluginInfo);
                }
            }

            OnPluginEnabled(new PluginEventArgs(pluginImplementation.PluginInfo));
        }

        public void DisablePlugin(PluginImplementation pluginImplementation)
        {
            lock (_plugins)
            {
                _logger.Debug("Disabling plugin {pluginInfo}", pluginImplementation.PluginInfo);

                // Device providers cannot be disabled at runtime simply queue a disable for next restart
                if (pluginImplementation is DeviceProvider)
                {
                    // Don't call SetEnabled(false) but simply update enabled state and save it
                    pluginImplementation.PluginInfo.IsEnabled = false;
                    pluginImplementation.PluginInfo.ApplyToEntity();
                    _pluginRepository.SavePlugin(pluginImplementation.PluginInfo.PluginEntity);
                    return;
                }

                pluginImplementation.SetEnabled(false);
                pluginImplementation.PluginInfo.ApplyToEntity();
                _pluginRepository.SavePlugin(pluginImplementation.PluginInfo.PluginEntity);

                _logger.Debug("Successfully disabled plugin {pluginInfo}", pluginImplementation.PluginInfo);
            }

            OnPluginDisabled(new PluginEventArgs(pluginImplementation.PluginInfo));
        }

        public PluginInfo GetPluginInfo(PluginImplementation pluginImplementation)
        {
            return _plugins.FirstOrDefault(p => p.Instance == pluginImplementation);
        }

        public List<PluginInfo> GetAllPluginInfo()
        {
            return new List<PluginInfo>(_plugins);
        }

        public List<T> GetPluginsOfType<T>() where T : PluginImplementation
        {
            return _plugins.Where(p => p.IsEnabled && p.Instance is T).Select(p => (T) p.Instance).ToList();
        }

        public PluginImplementation GetPluginByAssembly(Assembly assembly)
        {
            return _plugins.FirstOrDefault(p => p.Assembly == assembly)?.Instance;
        }

        public PluginImplementation GetPluginByDevice(IRGBDevice rgbDevice)
        {
            return GetPluginsOfType<DeviceProvider>().First(d => d.RgbDeviceProvider.Devices != null && d.RgbDeviceProvider.Devices.Contains(rgbDevice));
        }

        public PluginImplementation GetCallingPlugin()
        {
            StackTrace stackTrace = new StackTrace();           // get call stack
            StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)

            foreach (StackFrame stackFrame in stackFrames)
            {
                Assembly assembly = stackFrame.GetMethod().DeclaringType.Assembly;
                PluginImplementation pluginImplementation = GetPluginByAssembly(assembly);
                if (pluginImplementation != null)
                    return pluginImplementation;
            }

            return null;
        }

        public void Dispose()
        {
            UnloadPlugins();
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
        
        #region Events

        public event EventHandler CopyingBuildInPlugins;
        public event EventHandler<PluginEventArgs> PluginLoading;
        public event EventHandler<PluginEventArgs> PluginLoaded;
        public event EventHandler<PluginEventArgs> PluginUnloaded;
        public event EventHandler<PluginEventArgs> PluginEnabling;
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

        #endregion
    }
}