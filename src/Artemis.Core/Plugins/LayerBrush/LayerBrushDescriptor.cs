using System;

namespace Artemis.Core.Plugins.LayerBrush
{
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

        public string DisplayName { get; }
        public string Description { get; }
        public string Icon { get; }
        public Type LayerBrushType { get; }
        public LayerBrushProvider LayerBrushProvider { get; }
    }
}