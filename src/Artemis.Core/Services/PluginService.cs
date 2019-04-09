using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Artemis.Core.Events;
using Artemis.Core.Exceptions;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Plugins.Interfaces;
using Artemis.Core.Plugins.Models;
using Artemis.Core.ProfileElements;
using Artemis.Core.Services.Interfaces;
using Ninject;
using Ninject.Extensions.ChildKernel;

namespace Artemis.Core.Services
{
    public class PluginService : IPluginService
    {
        private readonly IKernel _kernel;
        private IKernel _childKernel;
        private AppDomain _appDomain;
        private readonly List<PluginInfo> _plugins;

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

            UnloadPlugins();

            // Create a child kernel and app domain that will only contain the plugins
            _childKernel = new ChildKernel(_kernel);
            _appDomain = AppDomain.CreateDomain("PluginAppDomain");
            
            // Load the plugin assemblies into the app domain
            var directory = new DirectoryInfo(Constants.DataFolder + "plugins");
            foreach (var subDirectory in directory.EnumerateDirectories())
            {
//                _appDomain.Load()
//                _plugins.Add(new PluginInfo(subDirectory.FullName));
            }

            OnFinishedLoadedPlugins();
        }

        private void UnloadPlugins()
        {
            _plugins.Clear();

            if (_childKernel != null)
            {
                _childKernel.Dispose();
                _childKernel = null;
            }
            if (_appDomain != null)
            {
                AppDomain.Unload(_appDomain);
                _appDomain = null;
            }
        }

        /// <inheritdoc />
        public async Task<IModuleViewModel> GetModuleViewModel(PluginInfo pluginInfo)
        {
            return await Task.Run(() => pluginInfo.GetModuleViewModel(_kernel));
        }

        /// <inheritdoc />
        public ILayerType GetLayerTypeByGuid(Guid layerTypeGuid)
        {
            var pluginInfo = _plugins.FirstOrDefault(p => p.Guid == layerTypeGuid);
            if (pluginInfo == null)
                return null;

            // Layer types are instantiated per layer so lets compile and return a new instance
            if (!(pluginInfo.Plugin is ILayerType))
            {
                throw new ArtemisPluginException(pluginInfo, "Plugin is expected to implement ILayerType");
            }

            return (ILayerType) pluginInfo.Plugin;
        }

        public void Dispose()
        {
            UnloadPlugins();
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