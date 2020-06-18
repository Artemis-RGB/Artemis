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
            var visualizationPath = new SKPath();
            visualizationPath.AddOval(SKRect.Create(0, 0, renderBounds.Bounds.Width / 2, renderBounds.Bounds.Height / 2));
            canvas.ClipPath(visualizationPath);
        }

        public override void PostProcess(SKCanvas canvas, SKImageInfo canvasInfo, SKPath renderBounds, SKPaint paint)
        {
            paint.ImageFilter = SKImageFilter.CreateDilate(
                (int) Properties.DilateRadius.CurrentValue.Width,
                (int) Properties.DilateRadius.CurrentValue.Height,
                paint.ImageFilter
            );
        }
    }
}