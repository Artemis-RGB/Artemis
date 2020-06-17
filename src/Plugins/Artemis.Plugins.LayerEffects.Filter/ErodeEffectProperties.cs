using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.Core.Models.Profile.LayerProperties.Types;
using SkiaSharp;

namespace Artemis.Plugins.LayerEffects.Filter
{
    public class ErodeEffectProperties : LayerPropertyGroup
    {
        [PropertyDescription(Description = "The amount of erode to apply", MinInputValue = 0)]
        public SKSizeLayerProperty ErodeRadius { get; set; }

        protected override void PopulateDefaults()
        {
        }

        protected override void OnPropertiesInitialized()
        {
        }
    }
}