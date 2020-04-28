using System;
using Artemis.Core.Utilities;

namespace Artemis.Core.Models.Profile.LayerProperties
{
    public class LayerPropertyKeyFrame<T>
    {
        private TimeSpan _position;

        public LayerPropertyKeyFrame(LayerProperty<T> layerProperty, T value, TimeSpan position, Easings.Functions easingFunction)
        {
            _position = position;
            Value = value;
            LayerProperty = layerProperty;
            EasingFunction = easingFunction;
        }

        public LayerProperty<T> LayerProperty { get; set; }
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