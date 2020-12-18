using System;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a keyframe on a <see cref="LayerProperty{T}" /> containing a value and a timestamp
    /// </summary>
    public class LayerPropertyKeyframe<T> : CorePropertyChanged, ILayerPropertyKeyframe
    {
        private LayerProperty<T> _layerProperty;
        private TimeSpan _position;
        private T _value;

        /// <summary>
        ///     Creates a new instance of the <see cref="LayerPropertyKeyframe{T}" /> class
        /// </summary>
        /// <param name="value">The value of the keyframe</param>
        /// <param name="position">The position of this keyframe in the timeline</param>
        /// <param name="easingFunction">The easing function applied on the value of the keyframe</param>
        /// <param name="layerProperty">The layer property this keyframe is applied to</param>
        public LayerPropertyKeyframe(T value, TimeSpan position, Easings.Functions easingFunction, LayerProperty<T> layerProperty)
        {
            _position = position;
            _layerProperty = layerProperty;
            _value = value;

            EasingFunction = easingFunction;
        }

        /// <summary>
        ///     The layer property this keyframe is applied to
        /// </summary>
        public LayerProperty<T> LayerProperty
        {
            get => _layerProperty;
            internal set => SetAndNotify(ref _layerProperty, value);
        }

        /// <summary>
        ///     The value of this keyframe
        /// </summary>
        public T Value
        {
            get => _value;
            set => SetAndNotify(ref _value, value);
        }

        /// <inheritdoc />
        public ILayerProperty UntypedLayerProperty => LayerProperty;
        
        /// <inheritdoc />
        public TimeSpan Position
        {
            get => _position;
            set
            {
                SetAndNotify(ref _position, value);
                LayerProperty.SortKeyframes();
            }
        }

        /// <inheritdoc />
        public Easings.Functions EasingFunction { get; set; }

        /// <inheritdoc />
        public KeyframeEntity GetKeyframeEntity()
        {
            return new()
            {
                Value = CoreJson.SerializeObject(Value),
                Position = Position,
                EasingFunction = (int) EasingFunction
            };
        }

        /// <inheritdoc />
        public void Remove()
        {
            LayerProperty.RemoveKeyframe(this);
        }
    }
}