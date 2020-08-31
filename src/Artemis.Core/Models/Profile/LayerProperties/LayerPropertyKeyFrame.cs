using System;

namespace Artemis.Core
{
    public class LayerPropertyKeyframe<T> : BaseLayerPropertyKeyframe
    {
        private LayerProperty<T> _layerProperty;
        private TimeSpan _position;
        private T _value;

        public LayerPropertyKeyframe(T value, TimeSpan position, Easings.Functions easingFunction, LayerProperty<T> layerProperty) : base(layerProperty)
        {
            _position = position;
            Value = value;
            LayerProperty = layerProperty;
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
        public override TimeSpan Position
        {
            get => _position;
            set
            {
                SetAndNotify(ref _position, value);
                LayerProperty.SortKeyframes();
            }
        }

        /// <inheritdoc />
        public override void Remove()
        {
            LayerProperty.RemoveKeyframe(this);
        }
    }
}