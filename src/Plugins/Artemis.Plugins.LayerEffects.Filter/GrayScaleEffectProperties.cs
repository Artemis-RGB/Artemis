using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.Core.Models.Profile.LayerProperties.Types;

namespace Artemis.Plugins.LayerEffects.Filter
{
    public class GrayScaleEffectProperties : LayerPropertyGroup
    {
        [PropertyDescription(Name = "Gray-scale strength (NYI)")]
        public FloatLayerProperty GrayScaleStrength { get; set; }

        protected override void PopulateDefaults()
        {
        }

        protected override void OnPropertiesInitialized()
        {
        }
    }
}