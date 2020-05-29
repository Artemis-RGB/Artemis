using System;
using Artemis.Core.Utilities;

namespace Artemis.Core.Models.Profile.LayerProperties
{
    public class LayerPropertyKeyframe<T> : BaseLayerPropertyKeyframe
    {
        private TimeSpan _position;

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
        public LayerProperty<T> LayerProperty { get; internal set; }

        /// <summary>
        ///     The value of this keyframe
        /// </summary>
        public T Value { get; set; }

        /// <inheritdoc />
        public override TimeSpan Position
        {
            get => _position;
            set
            {
                _position = value;
                LayerProperty.SortKeyframes();
            }
        }
    }
}