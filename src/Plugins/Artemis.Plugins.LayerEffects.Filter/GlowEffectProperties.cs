using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.Core.Models.Profile.LayerProperties.Types;
using SkiaSharp;

namespace Artemis.Plugins.LayerEffects.Filter
{
    public class GlowEffectProperties : LayerPropertyGroup
    {
        [PropertyDescription(Description = "The offset of the glow")]
        public SKPointLayerProperty GlowOffset { get; set; }

        [PropertyDescription(Description = "The amount of blur to apply to the glow")]
        public SKSizeLayerProperty GlowBlurAmount { get; set; }

        [PropertyDescription(Description = "The color of the glow")]
        public SKColorLayerProperty GlowColor { get; set; }

        protected override void PopulateDefaults()
        {
            GlowBlurAmount.DefaultValue = new SKSize(25, 25);
            GlowColor.DefaultValue = new SKColor(255, 255, 255);
        }

        protected override void OnPropertiesInitialized()
        {
        }
    }
}