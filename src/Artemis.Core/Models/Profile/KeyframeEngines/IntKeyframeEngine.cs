using System;
using System.Collections.Generic;
using Artemis.Core.Models.Profile.LayerProperties;

namespace Artemis.Core.Models.Profile.KeyframeEngines
{
    /// <inheritdoc />
    public class IntKeyframeEngine : KeyframeEngine
    {
        public sealed override List<Type> CompatibleTypes { get; } = new List<Type> {typeof(int)};

        protected override object GetInterpolatedValue()
        {
            var currentKeyframe = (Keyframe<int>) CurrentKeyframe;
            var nextKeyframe = (Keyframe<int>) NextKeyframe;

            var diff = nextKeyframe.Value - currentKeyframe.Value;
            return currentKeyframe.Value + diff * KeyframeProgress;
        }
    }
}