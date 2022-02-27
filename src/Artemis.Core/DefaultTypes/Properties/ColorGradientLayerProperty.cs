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
            DefaultValue = new ColorGradient();

            CurrentValueSet += OnCurrentValueSet;
        }

        private void CreateDataBindingRegistrations()
        {
            DataBinding.ClearDataBindingProperties();
            if (CurrentValue == null!)
                return;

            for (int index = 0; index < CurrentValue.Count; index++)
            {
                int stopIndex = index;

                void Setter(SKColor value)
                {
                    CurrentValue[stopIndex].Color = value;
                }

                DataBinding.RegisterDataBindingProperty(() => CurrentValue[stopIndex].Color, Setter, $"Color #{stopIndex + 1}");
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
            if (BaseValue == null!)
                BaseValue = new ColorGradient(DefaultValue);

            if (!ReferenceEquals(_subscribedGradient, BaseValue))
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
            if (CurrentValue.Count != DataBinding.Properties.Count)
                CreateDataBindingRegistrations();
        }

        #region Overrides of LayerProperty<ColorGradient>

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            // Don't allow color gradients to be null
            if (BaseValue == null!)
                BaseValue = new ColorGradient(DefaultValue);

            base.OnInitialize();
        }

        #endregion
    }
}