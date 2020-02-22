using System;
using System.IO;
using System.Reflection;
using Artemis.Core.Plugins.Abstract;
using McMaster.NETCore.Plugins;
using Newtonsoft.Json;

namespace Artemis.Core.Plugins.Models
{
    public class PluginInfo
    {
        internal PluginInfo()
        {
        }

        /// <summary>
        ///     The plugins GUID
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Guid Guid { get; internal set; }

        /// <summary>
        ///     The name of the plugin
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Name { get; internal set; }

        /// <summary>
        ///     The version of the plugin
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Version Version { get; internal set; }

        /// <summary>
        ///     The main entry DLL, should contain a class implementing Plugin
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Main { get; internal set; }

        /// <summary>
        ///     The plugins root directory
        /// </summary>
        [JsonIgnore]
        public DirectoryInfo Directory { get; internal set; }

        /// <summary>
        ///     A reference to the type implementing Plugin, available after successful load
        /// </summary>
        [JsonIgnore]
        public Plugin Instance { get; internal set; }

        /// <summary>
        ///     Indicates whether the user enabled the plugin or not
        /// </summary>
        [JsonIgnore]
        public bool Enabled { get; internal set; }

        /// <summary>
        ///     The PluginLoader backing this plugin
        /// </summary>
        [JsonIgnore]
        internal PluginLoader PluginLoader { get; set; }

        /// <summary>
        /// The assembly the plugin code lives in
        /// </summary>
        [JsonIgnore]
        internal Assembly Assembly { get; set; }

        public override string ToString()
        {
            return $"{nameof(Guid)}: {Guid}, {nameof(Name)}: {Name}, {nameof(Version)}: {Version}";
        }
    }
}