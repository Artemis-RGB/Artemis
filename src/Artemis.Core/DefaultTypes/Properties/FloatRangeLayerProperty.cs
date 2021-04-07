namespace Artemis.Core
{
    /// <inheritdoc />
    public class FloatRangeLayerProperty : LayerProperty<FloatRange>
    {
        internal FloatRangeLayerProperty()
        {
            RegisterDataBindingProperty(() => CurrentValue.Start, value => CurrentValue.Start = value, new FloatDataBindingConverter<FloatRange>(), "Start");
            RegisterDataBindingProperty(() => CurrentValue.End, value => CurrentValue.End = value, new FloatDataBindingConverter<FloatRange>(), "End");

            CurrentValueSet += OnCurrentValueSet;
            DefaultValue = new FloatRange();
        }

        /// <inheritdoc />
        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            float startDiff = NextKeyframe!.Value.Start - CurrentKeyframe!.Value.Start;
            float endDiff = NextKeyframe!.Value.End - CurrentKeyframe!.Value.End;
            CurrentValue = new FloatRange(
                (int) (CurrentKeyframe!.Value.Start + startDiff * keyframeProgressEased),
                (int) (CurrentKeyframe!.Value.End + endDiff * keyframeProgressEased)
            );
        }

        private void OnCurrentValueSet(object? sender, LayerPropertyEventArgs e)
        {
            // Don't allow the int range to be null
            BaseValue ??= DefaultValue ?? new FloatRange();
        }
    }
}