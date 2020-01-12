using System;
using System.Collections.Generic;
using Artemis.Core.Models.Profile.LayerProperties;
using SkiaSharp;

namespace Artemis.Core.Models.Profile.KeyframeEngines
{
    /// <inheritdoc />
    public class SKPointKeyframeEngine : KeyframeEngine
    {
        public sealed override List<Type> CompatibleTypes { get; } = new List<Type> {typeof(SKPoint)};

        protected override object GetInterpolatedValue()
        {
            var currentKeyframe = (Keyframe<SKPoint>) CurrentKeyframe;
            var nextKeyframe = (Keyframe<SKPoint>) NextKeyframe;

            var xDiff = nextKeyframe.Value.X - currentKeyframe.Value.X;
            var yDiff = nextKeyframe.Value.Y - currentKeyframe.Value.Y;
            return new SKPoint(currentKeyframe.Value.X + xDiff * KeyframeProgressEased, currentKeyframe.Value.Y + yDiff * KeyframeProgressEased);
        }
    }
}