using Artemis.Core.Exceptions;

namespace Artemis.Core.Models.Profile.LayerProperties.Types
{
    /// <summary>
    /// A special layer property used to configure the selected layer effect
    /// </summary>
    public class LayerEffectReferenceLayerProperty : LayerProperty<LayerEffectReference>
    {
        internal LayerEffectReferenceLayerProperty()
        {
            KeyframesSupported = false;
        }

        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            throw new ArtemisCoreException("Layer effect references do not support keyframes.");
        }

        public static implicit operator LayerEffectReference(LayerEffectReferenceLayerProperty p) => p.CurrentValue;
    }
}