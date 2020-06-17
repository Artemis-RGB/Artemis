using Artemis.Core.Plugins.LayerEffect.Abstract;
using SkiaSharp;

namespace Artemis.Plugins.LayerEffects.Filter
{
    public class DilateEffect : LayerEffect<DilateEffectProperties>
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

        public override void PreProcess(SKCanvas canvas, SKImageInfo canvasInfo, SKPath renderBounds, SKPaint paint)
        {
            paint.ImageFilter = SKImageFilter.CreateDilate(
                (int) Properties.DilateRadius.CurrentValue.Width,
                (int) Properties.DilateRadius.CurrentValue.Height,
                paint.ImageFilter
            );
        }

        public override void PostProcess(SKCanvas canvas, SKImageInfo canvasInfo, SKPath renderBounds, SKPaint paint)
        {
        }
    }
}