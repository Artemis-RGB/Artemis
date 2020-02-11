using System;
using System.Collections.Generic;
using Artemis.Core.Models.Profile.LayerProperties;
using SkiaSharp;

namespace Artemis.Core.Models.Profile.KeyframeEngines
{
    /// <inheritdoc />
    public class SKColorKeyframeEngine : KeyframeEngine
    {
        public sealed override List<Type> CompatibleTypes { get; } = new List<Type> {typeof(SKColor)};

        protected override object GetInterpolatedValue()
        {
            var currentKeyframe = (Keyframe<SKColor>) CurrentKeyframe;
            var nextKeyframe = (Keyframe<SKColor>) NextKeyframe;

            var redDiff = nextKeyframe.Value.Red - currentKeyframe.Value.Red;
            var greenDiff = nextKeyframe.Value.Green - currentKeyframe.Value.Green;
            var blueDiff = nextKeyframe.Value.Blue - currentKeyframe.Value.Blue;
            var alphaDiff = nextKeyframe.Value.Alpha - currentKeyframe.Value.Alpha;

            return new SKColor(
                (byte) (currentKeyframe.Value.Red + redDiff * KeyframeProgressEased),
                (byte) (currentKeyframe.Value.Green + greenDiff * KeyframeProgressEased),
                (byte) (currentKeyframe.Value.Blue + blueDiff * KeyframeProgressEased),
                (byte)(currentKeyframe.Value.Alpha + alphaDiff * KeyframeProgressEased)
            );
        }
    }
}