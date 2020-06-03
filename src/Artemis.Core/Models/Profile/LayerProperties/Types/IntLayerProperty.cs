using System;

namespace Artemis.Core.Models.Profile.LayerProperties.Types
{
    /// <inheritdoc/>
    public class IntLayerProperty : LayerProperty<int>
    {
        internal IntLayerProperty()
        {
        }

        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            var diff = NextKeyframe.Value - CurrentKeyframe.Value;
            CurrentValue = (int) Math.Round(CurrentKeyframe.Value + diff * keyframeProgressEased, MidpointRounding.AwayFromZero);
        }

        public static implicit operator int(IntLayerProperty p) => p.CurrentValue;
        public static implicit operator float(IntLayerProperty p) => p.CurrentValue;
        public static implicit operator double(IntLayerProperty p) => p.CurrentValue;
    }
}