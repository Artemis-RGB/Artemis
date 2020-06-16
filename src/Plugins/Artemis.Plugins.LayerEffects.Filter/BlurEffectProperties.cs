using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.Core.Models.Profile.LayerProperties.Types;

namespace Artemis.Plugins.LayerEffects.Filter
{
    public class BlurEffectProperties : LayerPropertyGroup
    {
        [PropertyDescription(Description = "The amount of blur to apply")]
        public SKSizeLayerProperty BlurAmount { get; set; }


        protected override void PopulateDefaults()
        {
        }

        protected override void OnPropertiesInitialized()
        {
        }

        private void UpdateVisibility()
        {
        }
    }
}