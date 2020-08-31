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
        }

        public static implicit operator LayerBrushReference(LayerBrushReferenceLayerProperty p)
        {
            return p.CurrentValue;
        }

        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            throw new ArtemisCoreException("Layer brush references do not support keyframes.");
        }
    }
}