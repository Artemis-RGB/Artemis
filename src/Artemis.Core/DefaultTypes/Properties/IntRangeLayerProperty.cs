namespace Artemis.Core
{
    /// <inheritdoc />
    public class IntRangeLayerProperty : LayerProperty<IntRange>
    {
        /// <inheritdoc />
        protected override void OnInitialize()
        {
            DataBinding.RegisterDataBindingProperty(() => CurrentValue.Start, value => CurrentValue = new IntRange(value, CurrentValue.End), "Start");
            DataBinding.RegisterDataBindingProperty(() => CurrentValue.End, value => CurrentValue = new IntRange(CurrentValue.Start, value), "End");
        }

        /// <inheritdoc />
        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            float startDiff = NextKeyframe!.Value.Start - CurrentKeyframe!.Value.Start;
            float endDiff = NextKeyframe!.Value.End - CurrentKeyframe!.Value.End;
            CurrentValue = new IntRange(
                (int) (CurrentKeyframe!.Value.Start + startDiff * keyframeProgressEased),
                (int) (CurrentKeyframe!.Value.End + endDiff * keyframeProgressEased)
            );
        }
    }
}