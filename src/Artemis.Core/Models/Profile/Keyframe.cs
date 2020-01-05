using System;

namespace Artemis.Core.Models.Profile
{
    public class Keyframe
    {
        public Layer Layer { get; set; }
        public LayerProperty Property { get; set; }

        public TimeSpan Position { get; set; }
        public object Value { get; set; }
    }
}