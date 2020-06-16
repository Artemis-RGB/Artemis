using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.Core.Models.Profile.LayerProperties.Types;

namespace Artemis.Plugins.LayerEffects.Filter
{
    public class DilateEffectProperties : LayerPropertyGroup
    {
        [PropertyDescription(Description = "The amount of dilation to apply")]
        public SKSizeLayerProperty DilateRadius { get; set; }

        protected override void PopulateDefaults()
        {
        }

        protected override void OnPropertiesInitialized()
        {
        }
    }
}