namespace Artemis.Core
{
    /// <inheritdoc />
    public class ColorGradientLayerProperty : LayerProperty<ColorGradient>
    {
        internal ColorGradientLayerProperty()
        {
            KeyframesSupported = false;
            DataBindingsSupported = false;
            DefaultValue = new ColorGradient();
            
            CurrentValueSet += OnCurrentValueSet;
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

        private void OnCurrentValueSet(object? sender, LayerPropertyEventArgs e)
        {
            // Don't allow color gradients to be null
            if (BaseValue == null)
                BaseValue = DefaultValue ?? new ColorGradient();
        }

        #region Overrides of LayerProperty<ColorGradient>

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            // Don't allow color gradients to be null
            if (BaseValue == null)
                BaseValue = DefaultValue ?? new ColorGradient();
            
            base.OnInitialize();
        }

        #endregion
    }
}