using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core.Events;
using Artemis.Core.Exceptions;
using Artemis.Core.Models;
using Artemis.Core.Services.Interfaces;
using Artemis.Plugins.Interfaces;
using CSScriptLibrary;
using Newtonsoft.Json;
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

        /// <summary>
        ///     Loads all installed plugins. If plugins already loaded this will reload them all
        /// </summary>
        /// <returns></returns>
        public async Task LoadPlugins()
        {
            if (LoadingPlugins)
                throw new ArtemisCoreException("Cannot load plugins while a previous load hasn't been completed yet.");

            OnStartedLoadingPlugins();

            // Empty the list of plugins
            _plugins.Clear();

            // Iterate all plugin folders and load each plugin
            foreach (var directory in Directory.GetDirectories(Constants.DataFolder + "plugins"))
                _plugins.Add(await LoadPluginFromFolder(directory));
            
            OnFinishedLoadedPlugins();
        }

        public async Task ReloadPlugin(PluginInfo pluginInfo)
        {
        }

        public async Task<IPluginViewModel> GetPluginViewModel(PluginInfo pluginInfo)
        {
            // Compile the ViewModel and get the type
            var compile = await Task.Run(() => CSScript.LoadFile(pluginInfo.Folder + pluginInfo.ViewModel));
            var vmType = compile.ExportedTypes.FirstOrDefault(t => typeof(IPluginViewModel).IsAssignableFrom(t));
            if (vmType == null)
                throw new ArtemisPluginException(pluginInfo, "Cannot locate a view model for this plugin.");
           
            // Instantiate the ViewModel with Ninject
            return (IPluginViewModel) _kernel.Get(vmType);
        }

        public void Dispose()
        {
        }

        private async Task<PluginInfo> LoadPluginFromFolder(string folder)
        {
            if (!folder.EndsWith("\\"))
                folder += "\\";
            if (!File.Exists(folder + "plugin.json"))
                throw new ArtemisPluginException(null, "Failed to load plugin, no plugin.json found in " + folder);

            var pluginInfo = JsonConvert.DeserializeObject<PluginInfo>(File.ReadAllText(folder + "plugin.json"));
            pluginInfo.Folder = folder;

            // Load the main plugin which will contain a class implementing IPlugin
            var plugin = await CSScript.Evaluator.LoadFileAsync<IPlugin>(folder + pluginInfo.Main);
            pluginInfo.Plugin = plugin;

            return pluginInfo;
        }

        #region Events

        /// <summary>
        ///     Occurs when a single plugin has loaded
        /// </summary>
        public event EventHandler<PluginEventArgs> PluginLoaded;

        /// <summary>
        ///     Occurs when a single plugin has reloaded
        /// </summary>
        public event EventHandler<PluginEventArgs> PluginReloaded;

        /// <summary>
        ///     Occurs when loading all plugins has started
        /// </summary>
        public event EventHandler StartedLoadingPlugins;

        /// <summary>
        ///     Occurs when loading all plugins has finished
        /// </summary>
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