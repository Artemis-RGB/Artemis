using System;
using Artemis.Core.Utilities;

namespace Artemis.Core.Models.Profile.LayerProperties
{
    public class LayerPropertyKeyframe<T> : BaseLayerPropertyKeyframe
    {
        private TimeSpan _position;

        public LayerPropertyKeyframe(T value, TimeSpan position, Easings.Functions easingFunction)
        {
            _position = position;
            Value = value;
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

        /// <inheritdoc />
        public sealed override Easings.Functions EasingFunction { get; set; }

        internal override BaseLayerProperty BaseLayerProperty => LayerProperty;
    }
}