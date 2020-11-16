namespace Artemis.Core
{
    /// <inheritdoc />
    public class IntRangeLayerProperty : LayerProperty<IntRange>
    {
        internal IntRangeLayerProperty()
        {
            RegisterDataBindingProperty(range => range.Start, new IntDataBindingConverter<IntRange>());
            RegisterDataBindingProperty(range => range.End, new IntDataBindingConverter<IntRange>());

            CurrentValueSet += OnCurrentValueSet;
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

        private void OnCurrentValueSet(object? sender, LayerPropertyEventArgs<IntRange> e)
        {
            // Don't allow the int range to be null
            BaseValue ??= DefaultValue ?? new IntRange(0, 0);
        }
    }
}