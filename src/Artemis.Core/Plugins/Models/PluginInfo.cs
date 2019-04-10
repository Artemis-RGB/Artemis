using System;
using System.Collections.Generic;
using Artemis.Core.Plugins.Interfaces;
using Newtonsoft.Json;

namespace Artemis.Core.Plugins.Models
{
    public class PluginInfo
    {
        /// <summary>
        ///     The plugins GUID
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
        ///     The main entry DLL, should contain a class implementing IPlugin
        /// </summary>
        public string Main { get; set; }

        /// <summary>
        ///     Full path to the plugin's current folder
        /// </summary>
        [JsonIgnore]
        public string Folder { get; set; }

        /// <summary>
        ///     A references to the types implementing IPlugin, available after successful load
        /// </summary>
        [JsonIgnore]
        public List<IPlugin> Instances { get; set; }

        public override string ToString()
        {
            return $"{nameof(Guid)}: {Guid}, {nameof(Name)}: {Name}, {nameof(Version)}: {Version}";
        }
    }
}