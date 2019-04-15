using System;
using System.IO;
using AppDomainToolkit;
using Artemis.Core.Plugins.Abstract;
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
        public Guid Guid { get; internal set; }

        /// <summary>
        ///     The name of the plugin
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        ///     The version of the plugin
        /// </summary>
        public string Version { get; internal set; }

        /// <summary>
        ///     The main entry DLL, should contain a class implementing Plugin
        /// </summary>
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
        ///     The AppDomain context of this plugin
        /// </summary>
        [JsonIgnore]
        internal AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> Context { get; set; }

        public override string ToString()
        {
            return $"{nameof(Guid)}: {Guid}, {nameof(Name)}: {Name}, {nameof(Version)}: {Version}";
        }
    }
}