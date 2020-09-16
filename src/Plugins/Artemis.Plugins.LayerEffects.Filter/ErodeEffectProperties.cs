using Artemis.Core;
using Artemis.Core.DefaultTypes;

namespace Artemis.Plugins.LayerEffects.Filter
{
    public class ErodeEffectProperties : LayerPropertyGroup
    {
        [PropertyDescription(Description = "The amount of erode to apply", MinInputValue = 0)]
        public SKSizeLayerProperty ErodeRadius { get; set; }

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