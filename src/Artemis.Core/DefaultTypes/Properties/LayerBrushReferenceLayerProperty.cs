namespace Artemis.Core
{
    /// <summary>
    ///     A special layer property used to configure the selected layer brush
    /// </summary>
    public class LayerBrushReferenceLayerProperty : LayerProperty<LayerBrushReference>
    {
        internal LayerBrushReferenceLayerProperty()
        {
            KeyframesSupported = false;
            DataBindingsSupported = false;
        }

        /// <summary>
        ///     Implicitly converts an <see cref="LayerBrushReferenceLayerProperty" /> to an <see cref="LayerBrushReference" />
        /// </summary>
        public static implicit operator LayerBrushReference(LayerBrushReferenceLayerProperty p)
        {
            return p.CurrentValue;
        }

        /// <inheritdoc />
        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            throw new ArtemisCoreException("Layer brush references do not support keyframes.");
        }
    }
}