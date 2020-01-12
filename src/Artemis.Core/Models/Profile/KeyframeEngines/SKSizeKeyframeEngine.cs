using System;
using System.Collections.Generic;
using Artemis.Core.Models.Profile.LayerProperties;
using SkiaSharp;

namespace Artemis.Core.Models.Profile.KeyframeEngines
{
    /// <inheritdoc />
    public class SKSizeKeyframeEngine : KeyframeEngine
    {
        public sealed override List<Type> CompatibleTypes { get; } = new List<Type> {typeof(SKSize)};

        protected override object GetInterpolatedValue()
        {
            var currentKeyframe = (Keyframe<SKSize>)CurrentKeyframe;
            var nextKeyframe = (Keyframe<SKSize>)NextKeyframe;

            var widthDiff = nextKeyframe.Value.Width - currentKeyframe.Value.Width;
            var heightDiff = nextKeyframe.Value.Height - currentKeyframe.Value.Height;
            return new SKSize(currentKeyframe.Value.Width + widthDiff * KeyframeProgressEased, currentKeyframe.Value.Height + heightDiff * KeyframeProgressEased);
        }
    }
}