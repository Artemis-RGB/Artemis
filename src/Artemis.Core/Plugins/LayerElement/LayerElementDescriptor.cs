using System;

namespace Artemis.Core.Plugins.LayerElement
{
    public class LayerElementDescriptor
    {
        internal LayerElementDescriptor(string displayName, string description, string icon, Type layerElementType, LayerElementProvider layerElementProvider)
        {
            DisplayName = displayName;
            Description = description;
            Icon = icon;
            LayerElementType = layerElementType;
            LayerElementProvider = layerElementProvider;
        }

        public string DisplayName { get; }
        public string Description { get; }
        public string Icon { get; }
        public Type LayerElementType { get; }
        public LayerElementProvider LayerElementProvider { get; }
    }
}