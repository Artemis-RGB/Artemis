using System;
using System.Collections.Generic;
using Artemis.Core.Models.Profile.LayerProperties;

namespace Artemis.Core.Models.Profile.KeyframeEngines
{
    /// <inheritdoc />
    public class FloatKeyframeEngine : KeyframeEngine
    {
        public sealed override List<Type> CompatibleTypes { get; } = new List<Type> {typeof(float)};

        protected override object GetInterpolatedValue()
        {
            var currentKeyframe = (Keyframe<float>) CurrentKeyframe;
            var nextKeyframe = (Keyframe<float>) NextKeyframe;

            var diff = nextKeyframe.Value - currentKeyframe.Value;
            return currentKeyframe.Value + diff * KeyframeProgressEased;
        }
    }
}