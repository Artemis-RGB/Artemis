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
    }
}