using Artemis.Plugins.Interfaces;
using Newtonsoft.Json;

namespace Artemis.Core.Models
{
    public class PluginInfo
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
    }
}