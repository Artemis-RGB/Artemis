using Artemis.Core.Plugins.LayerEffect.Abstract;
using SkiaSharp;

namespace Artemis.Plugins.LayerEffects.Filter
{
    public class GlowEffect : LayerEffect<GlowEffectProperties>
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
            paint.ImageFilter = SKImageFilter.CreateDropShadow(
                Properties.GlowOffset.CurrentValue.X,
                Properties.GlowOffset.CurrentValue.Y,
                Properties.GlowBlurAmount.CurrentValue.Width,
                Properties.GlowBlurAmount.CurrentValue.Height, Properties.GlowColor,
                SKDropShadowImageFilterShadowMode.DrawShadowAndForeground,
                paint.ImageFilter
            );
        }

        public override void PostProcess(SKCanvas canvas, SKImageInfo canvasInfo, SKPath renderBounds, SKPaint paint)
        {
        }
    }
}