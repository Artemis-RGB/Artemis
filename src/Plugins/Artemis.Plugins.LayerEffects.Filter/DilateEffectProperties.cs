using Artemis.Core;
using Artemis.Core.DefaultTypes;

namespace Artemis.Plugins.LayerEffects.Filter
{
    public class DilateEffectProperties : LayerPropertyGroup
    {
        [PropertyDescription(Description = "The amount of dilation to apply", MinInputValue = 0)]
        public SKSizeLayerProperty DilateRadius { get; set; }

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