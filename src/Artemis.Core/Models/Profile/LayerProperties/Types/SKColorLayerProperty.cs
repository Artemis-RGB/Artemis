using System;
using SkiaSharp;

namespace Artemis.Core.Models.Profile.LayerProperties.Types
{
    /// <inheritdoc/>
    public class SKColorLayerProperty : LayerProperty<SKColor>
    {
        internal SKColorLayerProperty()
        {
        }

        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            var redDiff = NextKeyframe.Value.Red - CurrentKeyframe.Value.Red;
            var greenDiff = NextKeyframe.Value.Green - CurrentKeyframe.Value.Green;
            var blueDiff = NextKeyframe.Value.Blue - CurrentKeyframe.Value.Blue;
            var alphaDiff = NextKeyframe.Value.Alpha - CurrentKeyframe.Value.Alpha;

            CurrentValue = new SKColor(
                ClampToByte(CurrentKeyframe.Value.Red + redDiff * keyframeProgressEased),
                ClampToByte(CurrentKeyframe.Value.Green + greenDiff * keyframeProgressEased),
                ClampToByte(CurrentKeyframe.Value.Blue + blueDiff * keyframeProgressEased),
                ClampToByte(CurrentKeyframe.Value.Alpha + alphaDiff * keyframeProgressEased)
            );
        }

        private static byte ClampToByte(float value)
        {
            return (byte) Math.Max(0, Math.Min(255, value));
        }

        public static implicit operator SKColor(SKColorLayerProperty p) => p.CurrentValue;
    }
}