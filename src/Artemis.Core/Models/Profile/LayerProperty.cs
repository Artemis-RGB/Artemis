using System;
using System.Collections.Generic;

namespace Artemis.Core.Models.Profile
{
    public class LayerProperty
    {
        internal LayerProperty(Layer layer, LayerProperty parent)
        {
            Layer = layer;
            Parent = parent;

            Children = new List<LayerProperty>();
        }

        public Layer Layer { get; }
        public LayerProperty Parent { get; }
        public List<LayerProperty> Children { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public Type Type { get; set; }
        public object BaseValue { get; set; }
        public List<Keyframe> Keyframes { get; set; }
    }
}