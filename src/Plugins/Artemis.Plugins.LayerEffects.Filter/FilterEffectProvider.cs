using Artemis.Core.Plugins.Abstract;

namespace Artemis.Plugins.LayerEffects.Filter
{
    public class FilterEffectProvider : LayerEffectProvider
    {
        public override void EnablePlugin()
        {
            AddLayerEffectDescriptor<BlurEffect>("Blur", "A layer effect providing a blur filter effect", "BlurOn");
            AddLayerEffectDescriptor<DilateEffect>("Dilate", "A layer effect providing a dilation filter effect", "EyePlus");
            AddLayerEffectDescriptor<ErodeEffect>("Erode", "A layer effect providing an erode filter effect", "EyeMinus");
            AddLayerEffectDescriptor<GlowEffect>("Glow", "A layer effect providing a glow filter effect", "BoxShadow");
            AddLayerEffectDescriptor<GrayScaleEffect>("Gray-scale", "A layer effect providing a gray-scale filter effect", "InvertColors");
        }

        public override void DisablePlugin()
        {
        }
    }
}