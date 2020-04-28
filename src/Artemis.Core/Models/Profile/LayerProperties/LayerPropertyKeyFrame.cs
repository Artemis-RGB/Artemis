using System;
using Artemis.Core.Utilities;

namespace Artemis.Core.Models.Profile.LayerProperties
{
    public class LayerPropertyKeyframe<T>
    {
        private TimeSpan _position;

        public LayerPropertyKeyframe(T value, TimeSpan position, Easings.Functions easingFunction)
        {
            _position = position;
            Value = value;
            EasingFunction = easingFunction;
        }

        public GenericLayerProperty<T> LayerProperty { get; internal set; }
        public T Value { get; set; }

        public TimeSpan Position
        {
            get => _position;
            set
            {
                _position = value;
                LayerProperty.SortKeyframes();
            }
        }

        public Easings.Functions EasingFunction { get; set; }
    }
}