using System.Collections.Specialized;
using System.ComponentModel;
using SkiaSharp;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class ColorGradientLayerProperty : LayerProperty<ColorGradient>
    {
        private ColorGradient? _subscribedGradient;

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

            for (int index = 0; index < CurrentValue.Count; index++)
            {
                int stopIndex = index;

                void Setter(SKColor value)
                {
                    CurrentValue[stopIndex].Color = value;
                }

                RegisterDataBindingProperty(() => CurrentValue[stopIndex].Color, Setter, new ColorStopDataBindingConverter(), $"Color #{stopIndex + 1}");
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

            if (_subscribedGradient != BaseValue)
            {
                if (_subscribedGradient != null)
                    _subscribedGradient.CollectionChanged -= SubscribedGradientOnPropertyChanged;
                _subscribedGradient = BaseValue;
                _subscribedGradient.CollectionChanged += SubscribedGradientOnPropertyChanged;
            }

            CreateDataBindingRegistrations();
        }

        private void SubscribedGradientOnPropertyChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (CurrentValue.Count != GetAllDataBindingRegistrations().Count)
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