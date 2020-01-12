using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Exceptions;
using Artemis.Core.Models.Profile.KeyframeEngines;
using Artemis.Core.Utilities;
using Artemis.Storage.Entities.Profile;
using Newtonsoft.Json;

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
        ///     Whether to expand this property by default, this is useful for important parent properties.
        /// </summary>
        public bool ExpandByDefault { get; set; }

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

        /// <summary>
        ///     A list of keyframes defining different values of the property in time, this list contains the untyped
        ///     <see cref="BaseKeyframe" />.
        /// </summary>
        public IReadOnlyCollection<BaseKeyframe> UntypedKeyframes => BaseKeyframes.AsReadOnly();

        public KeyframeEngine KeyframeEngine { get; set; }

        protected List<BaseKeyframe> BaseKeyframes { get; set; }

        protected internal object BaseValue
        {
            get => _baseValue;
            set
            {
                if (value != null && value.GetType() != Type)
                    throw new ArtemisCoreException($"Cannot set value of type {value.GetType()} on property {this}, expected type is {Type}.");
                if (!Equals(_baseValue, value))
                {
                    _baseValue = value;
                    OnValueChanged();
                }
            }
        }

        internal void ApplyToEntity()
        {
            var propertyEntity = Layer.LayerEntity.PropertyEntities.FirstOrDefault(p => p.Id == Id);
            if (propertyEntity == null)
            {
                propertyEntity = new PropertyEntity {Id = Id};
                Layer.LayerEntity.PropertyEntities.Add(propertyEntity);
            }

            propertyEntity.ValueType = Type.Name;
            propertyEntity.Value = JsonConvert.SerializeObject(BaseValue);

            propertyEntity.KeyframeEntities.Clear();
            foreach (var baseKeyframe in BaseKeyframes)
            {
                propertyEntity.KeyframeEntities.Add(new KeyframeEntity
                {
                    Position = baseKeyframe.Position,
                    Value = JsonConvert.SerializeObject(baseKeyframe.BaseValue),
                    EasingFunction = (int) baseKeyframe.EasingFunction
                });
            }
        }

        internal void ApplyToProperty(PropertyEntity propertyEntity)
        {
            BaseValue = DeserializePropertyValue(propertyEntity.Value);

            BaseKeyframes.Clear();
            foreach (var keyframeEntity in propertyEntity.KeyframeEntities.OrderBy(e => e.Position))
            {
                // Create a strongly typed keyframe or else it cannot be cast later on
                var keyframeType = typeof(Keyframe<>);
                var keyframe = (BaseKeyframe) Activator.CreateInstance(keyframeType.MakeGenericType(Type), Layer, this);
                keyframe.Position = keyframeEntity.Position;
                keyframe.BaseValue = DeserializePropertyValue(keyframeEntity.Value);
                keyframe.EasingFunction = (Easings.Functions) keyframeEntity.EasingFunction;

                BaseKeyframes.Add(keyframe);
            }
        }

        /// <summary>
        ///     Creates a new keyframe for this base property without knowing the type
        /// </summary>
        /// <returns></returns>
        public BaseKeyframe CreateNewKeyframe(TimeSpan position)
        {
            // Create a strongly typed keyframe or else it cannot be cast later on
            var keyframeType = typeof(Keyframe<>);
            var keyframe = (BaseKeyframe) Activator.CreateInstance(keyframeType.MakeGenericType(Type), Layer, this);
            keyframe.Position = position;
            keyframe.BaseValue = BaseValue;
            BaseKeyframes.Add(keyframe);
            SortKeyframes();

            return keyframe;
        }

        /// <summary>
        ///     Removes all keyframes from the property and sets the base value to the current value.
        /// </summary>
        public void ClearKeyframes()
        {
            BaseValue = KeyframeEngine.GetCurrentValue();
            BaseKeyframes.Clear();
        }

        internal void SortKeyframes()
        {
            BaseKeyframes = BaseKeyframes.OrderBy(k => k.Position).ToList();
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Name)}: {Name}, {nameof(Description)}: {Description}";
        }

        private object DeserializePropertyValue(string value)
        {
            if (value == "null")
                return Type.IsValueType ? Activator.CreateInstance(Type) : null;
            return JsonConvert.DeserializeObject(value, Type);
        }

        #region Events

        public event EventHandler<EventArgs> ValueChanged;

        protected virtual void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}