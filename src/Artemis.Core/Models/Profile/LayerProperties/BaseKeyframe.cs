using System;
using Artemis.Core.Utilities;

namespace Artemis.Core.Models.Profile.LayerProperties
{
    public class BaseKeyframe
    {
        private TimeSpan _position;

        protected BaseKeyframe(Layer layer, BaseLayerProperty property)
        {
            Layer = layer;
            BaseProperty = property;
        }

        public Layer Layer { get; set; }

        public TimeSpan Position
        {
            get => _position;
            set
            {
                if (value == _position) return;
                _position = value;
                BaseProperty.SortKeyframes();
            }
        }

        protected BaseLayerProperty BaseProperty { get; }
        protected internal object BaseValue { get; set; }
        public Easings.Functions EasingFunction { get; set; }
    }
}