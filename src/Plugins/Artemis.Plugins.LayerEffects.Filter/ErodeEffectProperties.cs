using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.Core.Models.Profile.LayerProperties.Types;

namespace Artemis.Plugins.LayerEffects.Filter
{
    public class ErodeEffectProperties : LayerPropertyGroup
    {
        [PropertyDescription(Description = "The amount of erode to apply")]
        public SKSizeLayerProperty ErodeRadius { get; set; }

        protected override void PopulateDefaults()
        {
        }

        protected override void OnPropertiesInitialized()
        {
        }
    }
}