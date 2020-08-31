using System;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class IntLayerProperty : LayerProperty<int>
    {
        internal IntLayerProperty()
        {
        }

        public static implicit operator int(IntLayerProperty p)
        {
            return p.CurrentValue;
        }

        public static implicit operator float(IntLayerProperty p)
        {
            return p.CurrentValue;
        }

        public static implicit operator double(IntLayerProperty p)
        {
            return p.CurrentValue;
        }

        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            var diff = NextKeyframe.Value - CurrentKeyframe.Value;
            CurrentValue = (int) Math.Round(CurrentKeyframe.Value + diff * keyframeProgressEased, MidpointRounding.AwayFromZero);
        }
    }
}