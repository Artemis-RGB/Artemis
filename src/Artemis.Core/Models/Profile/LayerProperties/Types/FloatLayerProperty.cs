namespace Artemis.Core.Models.Profile.LayerProperties.Types
{
    /// <inheritdoc/>
    public class FloatLayerProperty : LayerProperty<float>
    {
        internal FloatLayerProperty()
        {
        }

        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            var diff = NextKeyframe.Value - CurrentKeyframe.Value;
            CurrentValue = CurrentKeyframe.Value + diff * keyframeProgressEased;
        }
    }
}