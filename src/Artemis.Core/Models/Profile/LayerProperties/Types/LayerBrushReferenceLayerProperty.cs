using Artemis.Core.Exceptions;

namespace Artemis.Core.Models.Profile.LayerProperties.Types
{
    /// <summary>
    /// A special layer property used to configure the selected layer brush
    /// </summary>
    public class LayerBrushReferenceLayerProperty : GenericLayerProperty<LayerBrushReference>
    {
        internal LayerBrushReferenceLayerProperty()
        {
            KeyframesSupported = false;
        }

        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            throw new ArtemisCoreException("Layer brush references do not support keyframes.");
        }
    }
}