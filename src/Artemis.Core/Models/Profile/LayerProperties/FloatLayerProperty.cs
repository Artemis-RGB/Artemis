using Artemis.Core.Models.Profile.LayerProperties.Abstract;

namespace Artemis.Core.Models.Profile.LayerProperties
{
    public class FloatLayerProperty : LayerProperty<float>
    {
        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            var diff = NextKeyframe.Value - CurrentKeyframe.Value;
            CurrentValue = CurrentKeyframe.Value + diff * keyframeProgressEased;
        }
    }
}