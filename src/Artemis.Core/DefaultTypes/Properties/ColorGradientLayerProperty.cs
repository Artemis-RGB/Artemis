using SkiaSharp;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class ColorGradientLayerProperty : LayerProperty<ColorGradient>
    {
        internal ColorGradientLayerProperty()
        {
            KeyframesSupported = false;
            DataBindingsSupported = true;
            DefaultValue = new ColorGradient();

            CurrentValueSet += OnCurrentValueSet;
        }

        private void CreateDataBindingRegistrations()
        {
            ClearDataBindingProperties();
            if (CurrentValue == null)
                return;

            for (int index = 0; index < CurrentValue.Stops.Count; index++)
            {
                int stopIndex = index;
                RegisterDataBindingProperty(
                    () => CurrentValue.Stops[stopIndex].Color,
                    value => CurrentValue.Stops[stopIndex].Color = value,
                    new ColorStopDataBindingConverter(),
                    $"Color #{stopIndex + 1}"
                );
            }
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

            CreateDataBindingRegistrations();
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

    internal class ColorStopDataBindingConverter : DataBindingConverter<ColorGradient, SKColor>
    {
        public ColorStopDataBindingConverter()
        {
            SupportsInterpolate = true;
            SupportsSum = true;
        }

        /// <inheritdoc />
        public override SKColor Sum(SKColor a, SKColor b)
        {
            return a.Sum(b);
        }

        /// <inheritdoc />
        public override SKColor Interpolate(SKColor a, SKColor b, double progress)
        {
            return a.Interpolate(b, (float) progress);
        }
    }
}