using Artemis.Core.Plugins.LayerEffect.Abstract;
using SkiaSharp;

namespace Artemis.Plugins.LayerEffects.Filter
{
    public class ErodeEffect : LayerEffect<ErodeEffectProperties>
    {
        public override void EnableLayerEffect()
        {
        }

        public override void DisableLayerEffect()
        {
        }

        public override void Update(double deltaTime)
        {
        }

        public override void PreProcess(SKCanvas canvas, SKImageInfo canvasInfo, SKPath path, SKPaint paint)
        {
            paint.ImageFilter = SKImageFilter.CreateErode(
                (int) Properties.ErodeRadius.CurrentValue.Width,
                (int) Properties.ErodeRadius.CurrentValue.Height,
                paint.ImageFilter
            );
        }

        public override void PostProcess(SKCanvas canvas, SKImageInfo canvasInfo, SKPath path, SKPaint paint)
        {
        }
    }
}