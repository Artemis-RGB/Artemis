using Artemis.Core;
using Artemis.Core.DefaultTypes;

namespace Artemis.Plugins.LayerEffects.Filter
{
    public class GrayScaleEffectProperties : LayerPropertyGroup
    {
        [PropertyDescription(Name = "Gray-scale strength (NYI)")]
        public FloatLayerProperty GrayScaleStrength { get; set; }

        protected override void PopulateDefaults()
        {
        }

        protected override void EnableProperties()
        {
        }

        protected override void DisableProperties()
        {
        }
    }
}