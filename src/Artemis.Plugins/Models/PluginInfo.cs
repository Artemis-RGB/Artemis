using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Artemis.Plugins.Exceptions;
using Artemis.Plugins.Interfaces;
using CSScriptLibrary;
using Newtonsoft.Json;
using Ninject;

namespace Artemis.Plugins.Models
{
    public class PluginInfo : IDisposable
    {
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
        ///     The file implementing IPluginViewModel, loaded when opened in the UI
        /// </summary>
        public string ViewModel { get; set; }

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
        ///     Indicates wether this is a built-in plugin. Built-in plugins are precompiled and have no files
        /// </summary>
        [JsonIgnore]
        public bool IsBuiltIn { get; private set; }

        public static async Task<PluginInfo> FromFolder(IKernel kernel, string folder)
        {
            if (!folder.EndsWith("\\"))
                folder += "\\";
            if (!File.Exists(folder + "plugin.json"))
                throw new ArtemisPluginException(null, "Failed to load plugin, no plugin.json found in " + folder);

            var pluginInfo = JsonConvert.DeserializeObject<PluginInfo>(File.ReadAllText(folder + "plugin.json"));
            pluginInfo.Folder = folder;

            // Load the main plugin which will contain a class implementing IPlugin
            var assembly = await CSScript.Evaluator.CompileCodeAsync(File.ReadAllText(folder + pluginInfo.Main));
            var pluginType = assembly.GetTypes().Where(t => typeof(IPlugin).IsAssignableFrom(t)).ToList();
            if (!pluginType.Any())
                throw new ArtemisPluginException(pluginInfo, "Failed to load plugin, no type found that implements IPlugin");
            if (pluginType.Count > 1)
                throw new ArtemisPluginException(pluginInfo, "Failed to load plugin, more than one type found that implements IPlugin");

            pluginInfo.Plugin = (IPlugin) kernel.Get(pluginType.First());
            pluginInfo.Plugin.LoadPlugin();

            return pluginInfo;
        }

        public static PluginInfo FromBuiltInPlugin(IKernel kernel, IPlugin builtInPlugin)
        {
            var pluginInfo = new PluginInfo
            {
                Name = builtInPlugin.GetType().Name,
                Version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion,
                Plugin = builtInPlugin,
                IsBuiltIn = true
            };
            pluginInfo.Plugin.LoadPlugin();

            return pluginInfo;
        }

        public void Dispose()
        {
        }
    }
}