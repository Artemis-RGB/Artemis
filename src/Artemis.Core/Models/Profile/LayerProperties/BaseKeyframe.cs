using System;

namespace Artemis.Core.Models.Profile.LayerProperties
{
    public class BaseKeyframe
    {
        protected BaseKeyframe(Layer layer, BaseLayerProperty property)
        {
            Layer = layer;
            BaseProperty = property;
        }

        public Layer Layer { get; set; }
        public TimeSpan Position { get; set; }

        protected BaseLayerProperty BaseProperty { get; }
        protected object BaseValue { get; set; }
    }
}