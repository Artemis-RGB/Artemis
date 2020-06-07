using System;
using Artemis.Core.Plugins.Abstract;

namespace Artemis.Core.Plugins.LayerEffect
{
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

        public string DisplayName { get; }
        public string Description { get; }
        public string Icon { get; }
        public Type LayerEffectType { get; }
        public LayerEffectProvider LayerEffectProvider { get; }
    }
}