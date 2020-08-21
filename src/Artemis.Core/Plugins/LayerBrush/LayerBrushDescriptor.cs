using System;
using Artemis.Core.Plugins.Abstract;

namespace Artemis.Core.Plugins.LayerBrush
{
    /// <summary>
    ///     A class that describes a layer brush
    /// </summary>
    public class LayerBrushDescriptor
    {
        internal LayerBrushDescriptor(string displayName, string description, string icon, Type layerBrushType, LayerBrushProvider layerBrushProvider)
        {
            DisplayName = displayName;
            Description = description;
            Icon = icon;
            LayerBrushType = layerBrushType;
            LayerBrushProvider = layerBrushProvider;
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
        ///     The type of the layer brush
        /// </summary>
        public Type LayerBrushType { get; }

        /// <summary>
        ///     The plugin that provided this <see cref="LayerBrushDescriptor" />
        /// </summary>
        public LayerBrushProvider LayerBrushProvider { get; }
    }
}