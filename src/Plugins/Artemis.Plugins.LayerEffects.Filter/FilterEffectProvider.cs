using Artemis.Core.LayerEffects;

namespace Artemis.Plugins.LayerEffects.Filter
{
    public class FilterEffectProvider : LayerEffectProvider
    {
        public override void EnablePlugin()
        {
            RegisterLayerEffectDescriptor<BlurEffect>(
                "Blur",
                "A layer effect providing a blur filter effect. \r\nNote: CPU intensive, best to only use on small layers or for a short period of time.",
                "BlurOn"
            );
            RegisterLayerEffectDescriptor<DilateEffect>("Dilate", "A layer effect providing a dilation filter effect", "EyePlus");
            RegisterLayerEffectDescriptor<OpacityEffect>("Opacity", "A layer effect letting you change the opacity of all children", "Opacity");
            RegisterLayerEffectDescriptor<ErodeEffect>("Erode", "A layer effect providing an erode filter effect", "EyeMinus");
            RegisterLayerEffectDescriptor<GlowEffect>("Glow", "A layer effect providing a glow filter effect", "BoxShadow");
            RegisterLayerEffectDescriptor<GrayScaleEffect>("Gray-scale", "A layer effect providing a gray-scale filter effect", "InvertColors");
            RegisterLayerEffectDescriptor<ColorMatrixEffect>("Color matrix", "A layer effect allowing you to apply a custom color matrix", "Matrix");
        }

        public override void DisablePlugin()
        {
        }
    }
}