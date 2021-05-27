﻿using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.DataModelExpansions;
using Artemis.Core.DeviceProviders;
using Artemis.Core.LayerBrushes;
using Artemis.Core.LayerEffects;
using Artemis.Core.Modules;
using Artemis.Storage.Entities.Plugins;
using Humanizer;
using Newtonsoft.Json;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents basic info about a plugin feature and contains a reference to the instance of said feature
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class PluginFeatureInfo : CorePropertyChanged, IPrerequisitesSubject
    {
        private string? _description;
        private string? _icon;
        private PluginFeature? _instance;
        private string _name = null!;

        internal PluginFeatureInfo(Plugin plugin, Type featureType, PluginFeatureEntity pluginFeatureEntity, PluginFeatureAttribute? attribute)
        {
            Plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
            FeatureType = featureType ?? throw new ArgumentNullException(nameof(featureType));
            Entity = pluginFeatureEntity;

            Name = attribute?.Name ?? featureType.Name.Humanize(LetterCasing.Title);
            Description = attribute?.Description;
            Icon = attribute?.Icon;
            AlwaysEnabled = attribute?.AlwaysEnabled ?? false;

            if (Icon != null) return;
            if (typeof(DeviceProvider).IsAssignableFrom(featureType))
                Icon = "Devices";
            else if (typeof(Module).IsAssignableFrom(featureType))
                Icon = "VectorRectangle";
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
            AlwaysEnabled = attribute?.AlwaysEnabled ?? false;
            Instance = instance;

            if (Icon != null) return;
            Icon = Instance switch
            {
                DeviceProvider => "Devices",
                Module => "VectorRectangle",
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
        ///     Marks the feature to always be enabled as long as the plugin is enabled and cannot be disabled
        /// </summary>
        [JsonProperty]
        public bool AlwaysEnabled { get; }

        /// <summary>
        ///     Gets a boolean indicating whether the feature is enabled in persistent storage
        /// </summary>
        public bool EnabledInStorage => Entity.IsEnabled;

        /// <summary>
        ///     Gets the feature this info is associated with
        ///     <para>Note: <see langword="null" /> if the associated <see cref="Plugin" /> is disabled</para>
        /// </summary>
        public PluginFeature? Instance
        {
            get => _instance;
            internal set => SetAndNotify(ref _instance, value);
        }

        internal PluginFeatureEntity Entity { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Instance?.Id ?? "Uninitialized feature";
        }

        /// <inheritdoc />
        public List<PluginPrerequisite> Prerequisites { get; } = new();

        /// <inheritdoc />
        public bool ArePrerequisitesMet()
        {
            return Prerequisites.All(p => p.IsMet());
        }
    }
}