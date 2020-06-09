using Artemis.Core.Plugins.Abstract;

namespace Artemis.Plugins.LayerEffects.Filter
{
    public class FilterEffectProvider : LayerEffectProvider
    {
        public override void EnablePlugin()
        {
            AddLayerEffectDescriptor<FilterEffect>("Filter", "A layer effect providing different types of filters", "ImageFilterFrames");
        }

        public override void DisablePlugin()
        {
        }
    }
}