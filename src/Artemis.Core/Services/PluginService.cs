using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Artemis.Core.Events;
using Artemis.Core.Exceptions;
using Artemis.Core.Services.Interfaces;
using Artemis.Plugins.Interfaces;
using Artemis.Plugins.Models;
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