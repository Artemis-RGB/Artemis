using Artemis.Core.Exceptions;
using Artemis.Core.Models.Profile.Colors;

namespace Artemis.Core.Models.Profile.LayerProperties.Types
{
    /// <inheritdoc/>
    public class ColorGradientLayerProperty : LayerProperty<ColorGradient>
    {
        internal ColorGradientLayerProperty()
        {
            KeyframesSupported = false;
        }

        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            throw new ArtemisCoreException("Color Gradients do not support keyframes.");
        }
    }
}