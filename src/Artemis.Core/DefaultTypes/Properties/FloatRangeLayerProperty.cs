namespace Artemis.Core;

/// <inheritdoc />
public class FloatRangeLayerProperty : LayerProperty<FloatRange>
{
    /// <inheritdoc />
    protected override void OnInitialize()
    {
        DataBinding.RegisterDataBindingProperty(() => CurrentValue.Start, value => CurrentValue = new FloatRange(value, CurrentValue.End), "Start");
        DataBinding.RegisterDataBindingProperty(() => CurrentValue.End, value => CurrentValue = new FloatRange(CurrentValue.Start, value), "End");
    }

    /// <inheritdoc />
    protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
    {
        float startDiff = NextKeyframe!.Value.Start - CurrentKeyframe!.Value.Start;
        float endDiff = NextKeyframe!.Value.End - CurrentKeyframe!.Value.End;
        CurrentValue = new FloatRange(
            CurrentKeyframe!.Value.Start + startDiff * keyframeProgressEased,
            CurrentKeyframe!.Value.End + endDiff * keyframeProgressEased
        );
    }
}