using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Artemis.Core.Events;
using Artemis.Core.Exceptions;
using Artemis.Core.Models;
using Artemis.Core.Services.Interfaces;
using Artemis.Plugins.Interfaces;
using CSScriptLibrary;
using Newtonsoft.Json;

namespace Artemis.Core.Services
{
    public class PluginService : IPluginService
    {
        private readonly List<IPlugin> _modules;

        public PluginService()
        {
            _modules = new List<IPlugin>();

            if (!Directory.Exists(Constants.DataFolder + "modules"))
                Directory.CreateDirectory(Constants.DataFolder + "modules");
        }

        public bool LoadingModules { get; private set; }
        public ReadOnlyCollection<IPlugin> Modules => _modules.AsReadOnly();

        /// <summary>
        ///     Loads all installed modules. If modules already loaded this will reload them all
        /// </summary>
        /// <returns></returns>
        public async Task LoadModules()
        {
            if (LoadingModules)
                throw new ArtemisCoreException("Cannot load modules while a previous load hasn't been completed yet.");

            OnStartedLoadingModules();

            // Empty the list of modules
            _modules.Clear();
            // Iterate all module folders
            foreach (var directory in Directory.GetDirectories(Constants.DataFolder + "modules"))
                // Load each module  
                _modules.Add(await LoadModuleFromFolder(directory));


            OnFinishedLoadedModules();
        }

        public async Task ReloadModule(IPlugin plugin)
        {
        }

        public void Dispose()
        {
        }

        private async Task<IPlugin> LoadModuleFromFolder(string folder)
        {
            if (!folder.EndsWith("\\"))
                folder += "\\";
            if (!File.Exists(folder + "module.json"))
                throw new ArtemisModuleException(null, "Failed to load module, no module.json found in " + folder);

            var moduleInfo = JsonConvert.DeserializeObject<ModuleInfo>(File.ReadAllText(folder + "module.json"));
            // Load the main module which will contain a class implementing IModule
            var module = await CSScript.Evaluator.LoadFileAsync<IPlugin>(folder + moduleInfo.MainFile);
            return module;
        }

        #region Events

        /// <summary>
        ///     Occurs when a single module has loaded
        /// </summary>
        public event EventHandler<ModuleEventArgs> ModuleLoaded;

        /// <summary>
        ///     Occurs when a single module has reloaded
        /// </summary>
        public event EventHandler<ModuleEventArgs> ModuleReloaded;

        /// <summary>
        ///     Occurs when loading all modules has started
        /// </summary>
        public event EventHandler StartedLoadingModules;

        /// <summary>
        ///     Occurs when loading all modules has finished
        /// </summary>
        public event EventHandler FinishedLoadedModules;

        private void OnModuleLoaded(ModuleEventArgs e)
        {
            ModuleLoaded?.Invoke(this, e);
        }

        private void OnModuleReloaded(ModuleEventArgs e)
        {
            ModuleReloaded?.Invoke(this, e);
        }

        private void OnStartedLoadingModules()
        {
            LoadingModules = true;
            StartedLoadingModules?.Invoke(this, EventArgs.Empty);
        }

        private void OnFinishedLoadedModules()
        {
            LoadingModules = false;
            FinishedLoadedModules?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}