namespace Artemis.Core;

/// <inheritdoc />
public class ColorGradientLayerProperty : LayerProperty<ColorGradient>
{
    internal ColorGradientLayerProperty()
    {
        KeyframesSupported = false;
        DefaultValue = [];
    }

    /// <summary>
    ///     Implicitly converts an <see cref="ColorGradientLayerProperty" /> to a <see cref="ColorGradient" />
    /// </summary>
    public static implicit operator ColorGradient(ColorGradientLayerProperty p)
    {
        return p.CurrentValue;
    }

    /// <inheritdoc />
    protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
    {
        throw new ArtemisCoreException("Color Gradients do not support keyframes.");
    }

    #region Overrides of LayerProperty<ColorGradient>

    /// <inheritdoc />
    protected override void OnInitialize()
    {
        // Don't allow color gradients to be null
        if (BaseValue == null!)
            BaseValue = new ColorGradient(DefaultValue);

        DataBinding.RegisterDataBindingProperty(() => CurrentValue, value =>
        {
            if (value != null)
                CurrentValue = value;
        }, "Value");
    }

    #endregion
}