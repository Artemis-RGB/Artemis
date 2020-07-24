using Artemis.Core.Plugins.Abstract;

namespace Artemis.Plugins.LayerEffects.Filter
{
    public class FilterEffectProvider : LayerEffectProvider
    {
        public override void EnablePlugin()
        {
            AddLayerEffectDescriptor<BlurEffect>(
                "Blur",
                "A layer effect providing a blur filter effect. \r\nNote: CPU intensive, best to only use on small layers or for a short period of time.",
                "BlurOn"
            );
            AddLayerEffectDescriptor<DilateEffect>("Dilate", "A layer effect providing a dilation filter effect", "EyePlus");
            AddLayerEffectDescriptor<OpacityEffect>("Opacity", "A layer effect letting you change the opacity of all children", "Opacity");
            AddLayerEffectDescriptor<ErodeEffect>("Erode", "A layer effect providing an erode filter effect", "EyeMinus");
            AddLayerEffectDescriptor<GlowEffect>("Glow", "A layer effect providing a glow filter effect", "BoxShadow");
            AddLayerEffectDescriptor<GrayScaleEffect>("Gray-scale", "A layer effect providing a gray-scale filter effect", "InvertColors");
            AddLayerEffectDescriptor<ColorMatrixEffect>("Color matrix", "A layer effect allowing you to apply a custom color matrix", "Matrix");
        }

        public override void DisablePlugin()
        {
        }
    }
}