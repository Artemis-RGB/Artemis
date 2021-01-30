using Artemis.Core.DataModelExpansions;
using Artemis.Core.DeviceProviders;
using Artemis.Core.LayerBrushes;
using Artemis.Core.LayerEffects;
using Artemis.Core.Modules;
using Humanizer;
using Newtonsoft.Json;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents basic info about a plugin feature and contains a reference to the instance of said feature
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PluginFeatureInfo : CorePropertyChanged
    {
        private string? _description;
        private string? _icon;
        private string _name = null!;
        private PluginFeature _pluginFeature = null!;

        internal PluginFeatureInfo()
        {
        }

        internal PluginFeatureInfo(PluginFeature instance, PluginFeatureAttribute? attribute)
        {
            Name = attribute?.Name ?? instance.GetType().Name.Humanize(LetterCasing.Title);
            Description = attribute?.Description;
            Icon = attribute?.Icon;
            PluginFeature = instance;

            if (Icon != null) return;
            Icon = PluginFeature switch
            {
                BaseDataModelExpansion => "TableAdd",
                DeviceProvider => "Devices",
                ProfileModule => "VectorRectangle",
                Module => "GearBox",
                LayerBrushProvider => "Brush",
                LayerEffectProvider => "AutoAwesome",
                _ => "Plugin"
            };
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
        ///     available icons
        /// </summary>
        [JsonProperty]
        public string? Icon
        {
            get => _icon;
            set => SetAndNotify(ref _icon, value);
        }

        /// <summary>
        ///     Gets the plugin this info is associated with
        /// </summary>
        public PluginFeature PluginFeature
        {
            get => _pluginFeature;
            internal set => SetAndNotify(ref _pluginFeature, value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return PluginFeature.Id;
        }
    }
}