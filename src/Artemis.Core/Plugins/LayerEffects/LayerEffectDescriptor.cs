using System;

namespace Artemis.Core.Plugins.LayerEffects
{
    /// <summary>
    ///     A class that describes a layer effect
    /// </summary>
    public class LayerEffectDescriptor
    {
        internal LayerEffectDescriptor(string displayName, string description, string icon, Type layerEffectType, LayerEffectProvider layerEffectProvider)
        {
            DisplayName = displayName;
            Description = description;
            Icon = icon;
            LayerEffectType = layerEffectType;
            LayerEffectProvider = layerEffectProvider;
        }

        /// <summary>
        ///     The name that is displayed in the UI
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        ///     The description that is displayed in the UI
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     The Material icon to display in the UI, a full reference can be found
        ///     <see href="https://materialdesignicons.com">here</see>
        /// </summary>
        public string Icon { get; }

        /// <summary>
        ///     The type of the layer effect
        /// </summary>
        public Type LayerEffectType { get; }

        /// <summary>
        ///     The plugin that provided this <see cref="LayerEffectDescriptor" />
        /// </summary>
        public LayerEffectProvider LayerEffectProvider { get; }
    }
}