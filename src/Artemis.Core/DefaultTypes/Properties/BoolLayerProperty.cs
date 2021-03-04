namespace Artemis.Core
{
    /// <inheritdoc />
    public class BoolLayerProperty : LayerProperty<bool>
    {
        internal BoolLayerProperty()
        {
            KeyframesSupported = false;
            RegisterDataBindingProperty(() => CurrentValue, value => CurrentValue = value, new GeneralDataBindingConverter<bool>(), "Value");
        }

        /// <summary>
        ///     Implicitly converts an <see cref="BoolLayerProperty" /> to a <see cref="bool" />
        /// </summary>
        public static implicit operator bool(BoolLayerProperty p)
        {
            return p.CurrentValue;
        }

        /// <inheritdoc />
        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            throw new ArtemisCoreException("Boolean properties do not support keyframes.");
        }
    }
}