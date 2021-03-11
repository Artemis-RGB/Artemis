using System;
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
        private PluginFeature? _instance;
        private string _name = null!;

        internal PluginFeatureInfo(Plugin plugin, Type featureType, PluginFeatureAttribute? attribute)
        {
            Plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
            FeatureType = featureType ?? throw new ArgumentNullException(nameof(featureType));

            Name = attribute?.Name ?? featureType.Name.Humanize(LetterCasing.Title);
            Description = attribute?.Description;
            Icon = attribute?.Icon;

            if (Icon != null) return;
            if (typeof(BaseDataModelExpansion).IsAssignableFrom(featureType))
                Icon = "TableAdd";
            else if (typeof(DeviceProvider).IsAssignableFrom(featureType))
                Icon = "Devices";
            else if (typeof(ProfileModule).IsAssignableFrom(featureType))
                Icon = "VectorRectangle";
            else if (typeof(Module).IsAssignableFrom(featureType))
                Icon = "GearBox";
            else if (typeof(LayerBrushProvider).IsAssignableFrom(featureType))
                Icon = "Brush";
            else if (typeof(LayerEffectProvider).IsAssignableFrom(featureType))
                Icon = "AutoAwesome";
            else
                Icon = "Plugin";
        }

        internal PluginFeatureInfo(Plugin plugin, PluginFeatureAttribute? attribute, PluginFeature instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            Plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
            FeatureType = instance.GetType();

            Name = attribute?.Name ?? instance.GetType().Name.Humanize(LetterCasing.Title);
            Description = attribute?.Description;
            Icon = attribute?.Icon;
            Instance = instance;

            if (Icon != null) return;
            Icon = Instance switch
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
        ///     Gets the plugin this feature info is associated with
        /// </summary>
        public Plugin Plugin { get; }

        /// <summary>
        ///     Gets the type of the feature
        /// </summary>
        public Type FeatureType { get; }

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
        ///     Gets the feature this info is associated with
        /// </summary>
        public PluginFeature? Instance
        {
            get => _instance;
            internal set => SetAndNotify(ref _instance, value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Instance?.Id ?? "Uninitialized feature";
        }
    }
}