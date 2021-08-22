namespace Artemis.Core
{
    /// <inheritdoc />
    public class FloatRangeLayerProperty : LayerProperty<FloatRange>
    {
        internal FloatRangeLayerProperty()
        {
            CurrentValueSet += OnCurrentValueSet;
            DefaultValue = new FloatRange();
        }

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            DataBinding.RegisterDataBindingProperty(() => CurrentValue.Start, value => CurrentValue.Start = value, "Start");
            DataBinding.RegisterDataBindingProperty(() => CurrentValue.End, value => CurrentValue.End = value, "End");
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