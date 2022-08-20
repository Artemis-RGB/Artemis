namespace Artemis.Core
{
    /// <inheritdoc />
    public class FloatLayerProperty : LayerProperty<float>
    {
        internal FloatLayerProperty()
        {
        }

        /// <inheritdoc />
        protected override void OnInitialize()
        {
            DataBinding.RegisterDataBindingProperty(() => CurrentValue, value => CurrentValue = value, "Value");
        }

        /// <summary>
        ///     Implicitly converts an <see cref="FloatLayerProperty" /> to a <see cref="float" />
        /// </summary>
        public static implicit operator float(FloatLayerProperty p)
        {
            return p.CurrentValue;
        }

        /// <summary>
        ///     Implicitly converts an <see cref="FloatLayerProperty" /> to a <see cref="double" />
        /// </summary>
        public static implicit operator double(FloatLayerProperty p)
        {
            return p.CurrentValue;
        }

        /// <inheritdoc />
        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            float diff = NextKeyframe!.Value - CurrentKeyframe!.Value;
            CurrentValue = CurrentKeyframe!.Value + diff * keyframeProgressEased;
        }
    }
}