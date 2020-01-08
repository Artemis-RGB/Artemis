using System;
using System.Collections.Generic;
using Artemis.Core.Exceptions;

namespace Artemis.Core.Models.Profile.LayerProperties
{
    public abstract class BaseLayerProperty
    {
        private object _baseValue;

        protected BaseLayerProperty(Layer layer, BaseLayerProperty parent, string id, string name, string description, Type type)
        {
            Layer = layer;
            Parent = parent;
            Id = id;
            Name = name;
            Description = description;
            Type = type;

            Children = new List<BaseLayerProperty>();
            BaseKeyframes = new List<BaseKeyframe>();
        }

        /// <summary>
        ///     The layer this property applies to
        /// </summary>
        public Layer Layer { get; }

        /// <summary>
        ///     The parent property of this property.
        /// </summary>
        public BaseLayerProperty Parent { get; }

        /// <summary>
        ///     The child properties of this property.
        ///     <remarks>If the layer has children it cannot contain a value or keyframes.</remarks>
        /// </summary>
        public List<BaseLayerProperty> Children { get; set; }

        /// <summary>
        ///     A unique identifier for this property, a layer may not contain two properties with the same ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     The user-friendly name for this property, shown in the UI.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     The user-friendly description for this property, shown in the UI.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     An optional input prefix to show before input elements in the UI.
        /// </summary>
        public string InputPrefix { get; set; }

        /// <summary>
        ///     An optional input affix to show behind input elements in the UI.
        /// </summary>
        public string InputAffix { get; set; }

        /// <summary>
        ///     The type of value this layer property contains.
        /// </summary>
        public Type Type { get; set; }

        protected List<BaseKeyframe> BaseKeyframes { get; set; }
        /// <summary>
        ///     A list of keyframes defining different values of the property in time, this list contains the untyped <see cref="BaseKeyframe"/>.
        /// </summary>
        public IReadOnlyCollection<BaseKeyframe> UntypedKeyframes => BaseKeyframes.AsReadOnly();

        protected object BaseValue
        {
            get => _baseValue;
            set
            {
                if (value != null && value.GetType() != Type)
                    throw new ArtemisCoreException($"Cannot set value of type {value.GetType()} on property {this}, expected type is {Type}.");
                _baseValue = value;
            }
        }


        public void ApplyToEntity()
        {
            // Big o' TODO
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Name)}: {Name}, {nameof(Description)}: {Description}";
        }
    }
}