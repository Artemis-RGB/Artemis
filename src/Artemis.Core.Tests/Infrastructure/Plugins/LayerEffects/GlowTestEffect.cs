using Artemis.Core.LayerEffects;
using SkiaSharp;

namespace Artemis.Core.Tests.Infrastructure.Plugins.LayerEffects;

/// <summary>
///     Grows whatever the element renders outwards, in pre-processing. Real glow effects blur, this uses a dilate filter so
///     the edge of the glow is crisp and predictable.
/// </summary>
public class GlowTestEffect : LayerEffect<GlowTestEffectProperties>
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

    public override void PreProcess(SKCanvas canvas, SKRect renderBounds, SKPaint paint)
    {
        float radius = Properties.Radius.CurrentValue;
        paint.ImageFilter = SKImageFilter.CreateDilate(radius, radius, paint.ImageFilter);
    }

    public override void PostProcess(SKCanvas canvas, SKRect renderBounds, SKPaint paint)
    {
    }
}
