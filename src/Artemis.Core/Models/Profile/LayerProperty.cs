using System;
using System.Collections.Generic;
using Artemis.Core.Exceptions;

namespace Artemis.Core.Models.Profile
{
    public class LayerProperty
    {
        private object _baseValue;

        internal LayerProperty(Layer layer, LayerProperty parent, string name, string description, Type type)
        {
            Layer = layer;
            Parent = parent;
            Name = name;
            Description = description;
            Type = type;

            Children = new List<LayerProperty>();
            Keyframes = new List<Keyframe>();
        }

        public Layer Layer { get; }
        public LayerProperty Parent { get; }
        public List<LayerProperty> Children { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public Type Type { get; set; }

        public object BaseValue
        {
            get => _baseValue;
            set
            {
                if (value != null && value.GetType() != Type)
                    throw new ArtemisCoreException($"Cannot set value of type {value.GetType()} on property {Name}, expected type is {Type}.");
                _baseValue = value;
            }
        }

        public List<Keyframe> Keyframes { get; set; }

        public void ApplyToEntity()
        {
            // Big o' TODO
        }
    }
}