using System;
using SkiaSharp;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class SKColorLayerProperty : LayerProperty<SKColor>
    {
        internal SKColorLayerProperty()
        {
        }

        public static implicit operator SKColor(SKColorLayerProperty p)
        {
            return p.CurrentValue;
        }

        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            CurrentValue = CurrentKeyframe.Value.Interpolate(NextKeyframe.Value, keyframeProgressEased);
        }

        private static byte ClampToByte(float value)
        {
            return (byte) Math.Max(0, Math.Min(255, value));
        }
    }
}