using System;
using Artemis.Core.Models.Profile.LayerProperties.Abstract;

namespace Artemis.Core.Models.Profile.LayerProperties
{
    public class IntLayerProperty : LayerProperty<int>
    {
        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            var diff = NextKeyframe.Value - CurrentKeyframe.Value;
            CurrentValue = (int) Math.Round(CurrentKeyframe.Value + diff * keyframeProgressEased, MidpointRounding.AwayFromZero);
        }
    }
}