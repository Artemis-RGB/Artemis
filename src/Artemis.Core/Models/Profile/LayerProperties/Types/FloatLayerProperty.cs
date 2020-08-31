namespace Artemis.Core
{
    /// <inheritdoc />
    public class FloatLayerProperty : LayerProperty<float>
    {
        internal FloatLayerProperty()
        {
        }

        public static implicit operator float(FloatLayerProperty p)
        {
            return p.CurrentValue;
        }

        public static implicit operator double(FloatLayerProperty p)
        {
            return p.CurrentValue;
        }

        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            var diff = NextKeyframe.Value - CurrentKeyframe.Value;
            CurrentValue = CurrentKeyframe.Value + diff * keyframeProgressEased;
        }
    }
}