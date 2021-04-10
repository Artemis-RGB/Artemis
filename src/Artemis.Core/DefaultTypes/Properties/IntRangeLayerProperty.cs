namespace Artemis.Core
{
    /// <inheritdoc />
    public class IntRangeLayerProperty : LayerProperty<IntRange>
    {
        internal IntRangeLayerProperty()
        {
            RegisterDataBindingProperty(() => CurrentValue.Start, value => CurrentValue.Start = value, new IntDataBindingConverter<IntRange>(), "Start");
            RegisterDataBindingProperty(() => CurrentValue.End, value => CurrentValue.End = value, new IntDataBindingConverter<IntRange>(), "End");

            CurrentValueSet += OnCurrentValueSet;
            DefaultValue = new IntRange();
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

        private void OnCurrentValueSet(object? sender, LayerPropertyEventArgs e)
        {
            // Don't allow the int range to be null
            BaseValue ??= DefaultValue ?? new IntRange();
        }
    }
}