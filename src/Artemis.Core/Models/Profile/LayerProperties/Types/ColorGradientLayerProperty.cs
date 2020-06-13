using Artemis.Core.Exceptions;
using Artemis.Core.Models.Profile.Colors;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core.Models.Profile.LayerProperties.Types
{
    /// <inheritdoc />
    public class ColorGradientLayerProperty : LayerProperty<ColorGradient>
    {
        internal ColorGradientLayerProperty()
        {
            KeyframesSupported = false;
        }

        public static implicit operator ColorGradient(ColorGradientLayerProperty p)
        {
            return p.CurrentValue;
        }

        protected override void UpdateCurrentValue(float keyframeProgress, float keyframeProgressEased)
        {
            throw new ArtemisCoreException("Color Gradients do not support keyframes.");
        }

        internal override void ApplyToLayerProperty(PropertyEntity entity, LayerPropertyGroup layerPropertyGroup, bool fromStorage)
        {
            base.ApplyToLayerProperty(entity, layerPropertyGroup, fromStorage);

            // Don't allow color gradients to be null
            BaseValue ??= DefaultValue ?? new ColorGradient();
        }
    }
}