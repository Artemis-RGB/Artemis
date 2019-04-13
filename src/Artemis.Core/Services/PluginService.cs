using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AppDomainToolkit;
using Artemis.Core.Events;
using Artemis.Core.Exceptions;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Plugins.Interfaces;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using Newtonsoft.Json;
using Ninject;
using Ninject.Extensions.ChildKernel;

namespace Artemis.Core.Services
{
    public class PluginService : IPluginService
    {
        private readonly IKernel _kernel;
        private readonly List<PluginInfo> _plugins;
        private IKernel _childKernel;

        public PluginService(IKernel kernel)
        {
            _kernel = kernel;
            _plugins = new List<PluginInfo>();

            if (!Directory.Exists(Constants.DataFolder + "plugins"))
                Directory.CreateDirectory(Constants.DataFolder + "plugins");
        }

        public bool LoadingPlugins { get; private set; }
        public ReadOnlyCollection<PluginInfo> Plugins => _plugins.AsReadOnly();

        /// <inheritdoc />
        public async Task LoadPlugins()
        {
            if (LoadingPlugins)
                throw new ArtemisCoreException("Cannot load plugins while a previous load hasn't been completed yet.");

            OnStartedLoadingPlugins();

            await Task.Run(() =>
            {
                UnloadPlugins();

                // Create a child kernel and app domain that will only contain the plugins
                _childKernel = new ChildKernel(_kernel);

                // Load the plugin assemblies into the plugin context
                var directory = new DirectoryInfo(Path.Combine(Constants.DataFolder, "plugins"));
                foreach (var subDirectory in directory.EnumerateDirectories())
                    try
                    {
                        // Load the metadata
                        var metadataFile = Path.Combine(subDirectory.FullName, "plugin.json");
                        if (!File.Exists(metadataFile))
                            throw new ArtemisPluginException("Couldn't find the plugins metadata file at " + metadataFile);

                        // Locate the main entry
                        var pluginInfo = JsonConvert.DeserializeObject<PluginInfo>(File.ReadAllText(metadataFile));
                        // TODO Just temporarily until settings are in place
                        pluginInfo.Enabled = true;
                        var mainFile = Path.Combine(subDirectory.FullName, pluginInfo.Main);
                        if (!File.Exists(mainFile))
                            throw new ArtemisPluginException(pluginInfo, "Couldn't find the plugins main entry at " + mainFile);

                        // Load the plugin, all types implementing IPlugin and register them with DI
                        var setupInfo = new AppDomainSetup
                        {
                            ApplicationName = pluginInfo.Guid.ToString(),
                            ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                            PrivateBinPath = subDirectory.FullName
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

                        // Get the IPlugin implementation from the main assembly and if there is only one, instantiate it
                        var mainAssembly = pluginInfo.Context.Domain.GetAssemblies().First(a => a.Location == mainFile);
                        var pluginTypes = mainAssembly.GetTypes().Where(t => typeof(IPlugin).IsAssignableFrom(t)).ToList();
                        if (pluginTypes.Count > 1)
                            throw new ArtemisPluginException(pluginInfo, $"Plugin contains {pluginTypes.Count} implementations of IPlugin, only 1 allowed");
                        if (pluginTypes.Count == 0)
                            throw new ArtemisPluginException(pluginInfo, "Plugin contains no implementation of IPlugin");

                        var pluginType = pluginTypes.Single();
                        _childKernel.Bind<IPlugin>().To(pluginType).InSingletonScope();
                        try
                        {
                            pluginInfo.Instance = (IPlugin) _childKernel.Get(pluginType);
                        }
                        catch (Exception e)
                        {
                            throw new ArtemisPluginException(pluginInfo, "Failed to instantiate the plugin", e);
                        }
                        _plugins.Add(pluginInfo);
                    }
                    catch (Exception e)
                    {
                        throw new ArtemisPluginException("Failed to load plugin", e);
                    }

                // Activate plugins after they are all loaded
                foreach (var pluginInfo in _plugins.Where(p => p.Enabled))
                    pluginInfo.Instance.EnablePlugin();
            });

            OnFinishedLoadedPlugins();
        }

        /// <inheritdoc />
        public ILayerType GetLayerTypeByGuid(Guid layerTypeGuid)
        {
            var pluginInfo = _plugins.FirstOrDefault(p => p.Guid == layerTypeGuid);
            if (pluginInfo == null)
                return null;

            if (!(pluginInfo.Instance is ILayerType layerType))
                throw new ArtemisPluginException(pluginInfo, "Plugin is expected to implement exactly one ILayerType");

            return layerType;
        }

        public IReadOnlyList<IModule> GetModules()
        {
            return Plugins.Where(p => p.Instance is IModule).Select(p => (IModule) p.Instance).ToList();
        }

        public void Dispose()
        {
            UnloadPlugins();
        }

        private void UnloadPlugins()
        {
            _plugins.Clear();

            if (_childKernel != null)
            {
                _childKernel.Dispose();
                _childKernel = null;
            }
        }

        #region Events

        public event EventHandler<PluginEventArgs> PluginLoaded;
        public event EventHandler StartedLoadingPlugins;
        public event EventHandler FinishedLoadedPlugins;

        private void OnPluginLoaded(PluginEventArgs e)
        {
            PluginLoaded?.Invoke(this, e);
        }

        private void OnStartedLoadingPlugins()
        {
            LoadingPlugins = true;
            StartedLoadingPlugins?.Invoke(this, EventArgs.Empty);
        }

        private void OnFinishedLoadedPlugins()
        {
            LoadingPlugins = false;
            FinishedLoadedPlugins?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}