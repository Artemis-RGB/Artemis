using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents basic info about a plugin and contains a reference to the instance of said plugin
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PluginInfo : CorePropertyChanged
    {
        private bool _autoEnableFeatures = true;
        private string? _description;
        private Guid _guid;
        private string? _icon;
        private string _main = null!;
        private string _name = null!;
        private Plugin _plugin = null!;
        private bool _requiresAdmin;
        private Version _version = null!;

        internal PluginInfo()
        {
        }

        /// <summary>
        ///     The plugins GUID
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Guid Guid
        {
            get => _guid;
            internal set => SetAndNotify(ref _guid, value);
        }

        /// <summary>
        ///     The name of the plugin
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Name
        {
            get => _name;
            internal set => SetAndNotify(ref _name, value);
        }

        /// <summary>
        ///     A short description of the plugin
        /// </summary>
        [JsonProperty]
        public string? Description
        {
            get => _description;
            set => SetAndNotify(ref _description, value);
        }

        /// <summary>
        ///     The plugins display icon that's shown in the settings see <see href="https://materialdesignicons.com" /> for
        ///     available
        ///     icons
        /// </summary>
        [JsonProperty]
        public string? Icon
        {
            get => _icon;
            set => SetAndNotify(ref _icon, value);
        }

        /// <summary>
        ///     The version of the plugin
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public Version Version
        {
            get => _version;
            internal set => SetAndNotify(ref _version, value);
        }

        /// <summary>
        ///     The main entry DLL, should contain a class implementing Plugin
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Main
        {
            get => _main;
            internal set => SetAndNotify(ref _main, value);
        }

        /// <summary>
        ///     Gets or sets a boolean indicating whether this plugin should automatically enable all its features when it is first
        ///     loaded
        /// </summary>
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool AutoEnableFeatures
        {
            get => _autoEnableFeatures;
            set => SetAndNotify(ref _autoEnableFeatures, value);
        }

        /// <summary>
        ///     Gets a boolean indicating whether this plugin requires elevated admin privileges
        /// </summary>
        [JsonProperty]
        public bool RequiresAdmin
        {
            get => _requiresAdmin;
            internal set => SetAndNotify(ref _requiresAdmin, value);
        }

        /// <summary>
        ///     Gets the plugin this info is associated with
        /// </summary>
        public Plugin Plugin
        {
            get => _plugin;
            internal set => SetAndNotify(ref _plugin, value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name} v{Version} - {Guid}";
        }
    }
}