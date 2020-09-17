namespace Artemis.Core.DefaultTypes
{
    /// <inheritdoc />
    public class BoolLayerProperty : LayerProperty<bool>
    {
        internal BoolLayerProperty()
        {
            KeyframesSupported = false;
            DataBindingsSupported = false;
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