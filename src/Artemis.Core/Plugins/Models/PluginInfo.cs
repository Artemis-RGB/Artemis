using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Plugins.Interfaces;
using CSScriptLibrary;
using Newtonsoft.Json;
using Ninject;

namespace Artemis.Core.Plugins.Models
{
    public class PluginInfo
    {
        private static Assembly _assembly;

        /// <summary>
        ///     The plugin's GUID
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        ///     The name of the plugin
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     The version of the plugin
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        ///     The file implementing IPlugin, loaded on startup
        /// </summary>
        public string Main { get; set; }

        /// <summary>
        ///     The instantiated plugin, available after successful load
        /// </summary>
        [JsonIgnore]
        public IPlugin Plugin { get; set; }

        /// <summary>
        ///     Full path to the plugin's current folder
        /// </summary>
        [JsonIgnore]
        public string Folder { get; set; }

        /// <summary>
        ///     Unloads the plugin and clears the plugin info's internal data
        /// </summary>
        public void UnloadPlugin()
        {
            Plugin.Dispose();
            _assembly = null;
        }

        /// <summary>
        ///     Load a plugin from a folder
        /// </summary>
        /// <param name="kernel">The Ninject kernel to use for DI</param>
        /// <param name="folder">The folder in which plugin.json is located</param>
        /// <returns></returns>
        public static async Task<PluginInfo> FromFolder(IKernel kernel, string folder)
        {
            // Make sure the right engine is used
            CSScript.EvaluatorConfig.Engine = EvaluatorEngine.CodeDom;
            CSScript.EvaluatorConfig.DebugBuild = true;
            CSScript.GlobalSettings.SearchDirs = folder;
            if (!folder.EndsWith("\\"))
                folder += "\\";
            if (!File.Exists(folder + "plugin.json"))
                throw new ArtemisPluginException(null, "Failed to load plugin, no plugin.json found in " + folder);

            var pluginInfo = JsonConvert.DeserializeObject<PluginInfo>(File.ReadAllText(folder + "plugin.json"));
            pluginInfo.Folder = folder;

            await pluginInfo.CompilePlugin(kernel);
            return pluginInfo;
        }

        /// <summary>
        ///     Compiles the plugin's main CS file and any of it's includes and instantiates it.
        /// </summary>
        /// <param name="kernel">The Ninject kernel to use for DI</param>
        public async Task CompilePlugin(IKernel kernel)
        {
            // Load the main script and get the type
            _assembly = await CSScript.Evaluator.CompileCodeAsync(File.ReadAllText(Folder + Main));

            var pluginType = _assembly.GetTypes().Where(t => typeof(IPlugin).IsAssignableFrom(t)).ToList();
            if (!pluginType.Any())
                throw new ArtemisPluginException(this, "Failed to load plugin, no type found that implements IPlugin");
            if (pluginType.Count > 1)
                throw new ArtemisPluginException(this, "Failed to load plugin, more than one type found that implements IPlugin");

            // Instantiate the plugin with Ninject
            Plugin = (IPlugin) kernel.Get(pluginType.First());
            Plugin.LoadPlugin();
        }

        /// <summary>
        ///     Gets the view model of the module accompanying the provided plugin info
        /// </summary>
        /// <param name="kernel">The Ninject kernel to use for DI</param>
        /// <returns></returns>
        public IModuleViewModel GetModuleViewModel(IKernel kernel)
        {
            // Don't attempt to locave VMs for something other than a module
            if (Plugin == null)
                throw new ArtemisPluginException(this, "Cannot locate a view model for this plugin because it's not compiled.");
            if (!(Plugin is IModule module))
                throw new ArtemisPluginException(this, "Cannot locate a view model for this plugin as it's not a module.");

            // Get the type from the module
            var vmType = module.ViewModelType;
            if (!typeof(IModuleViewModel).IsAssignableFrom(vmType))
                throw new ArtemisPluginException(this, "ViewModel must implement IModuleViewModel.");

            // Instantiate the ViewModel with Ninject
            var vm = (IModuleViewModel) kernel.Get(vmType);
            vm.PluginInfo = this;
            return vm;
        }
    }
}