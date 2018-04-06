using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Events;
using Artemis.Core.Exceptions;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Plugins.Interfaces;
using Artemis.Core.Plugins.Models;
using Artemis.Core.ProfileElements;
using Artemis.Core.Services.Interfaces;
using Ninject;

namespace Artemis.Core.Services
{
    public class PluginService : IPluginService
    {
        private readonly IKernel _kernel;
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

            // Empty the list of plugins
            foreach (var pluginInfo in _plugins)
                pluginInfo.UnloadPlugin();
            _plugins.Clear();

            // Iterate all plugin folders and load each plugin
            foreach (var directory in Directory.GetDirectories(Constants.DataFolder + "plugins"))
                _plugins.Add(await PluginInfo.FromFolder(_kernel, directory));

            OnFinishedLoadedPlugins();
        }

        /// <inheritdoc />
        public async Task ReloadPlugin(PluginInfo pluginInfo)
        {
            pluginInfo.UnloadPlugin();
            await pluginInfo.CompilePlugin(_kernel);

            OnPluginReloaded(new PluginEventArgs(pluginInfo));
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
            // Empty the list of plugins
            foreach (var pluginInfo in _plugins)
                pluginInfo.UnloadPlugin();
            _plugins.Clear();
        }

        #region Events

        public event EventHandler<PluginEventArgs> PluginLoaded;
        public event EventHandler<PluginEventArgs> PluginReloaded;
        public event EventHandler StartedLoadingPlugins;
        public event EventHandler FinishedLoadedPlugins;

        private void OnPluginLoaded(PluginEventArgs e)
        {
            PluginLoaded?.Invoke(this, e);
        }

        private void OnPluginReloaded(PluginEventArgs e)
        {
            PluginReloaded?.Invoke(this, e);
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