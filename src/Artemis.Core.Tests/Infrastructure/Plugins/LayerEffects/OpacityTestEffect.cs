using Artemis.Core.LayerEffects;
using SkiaSharp;

namespace Artemis.Core.Tests.Infrastructure.Plugins.LayerEffects;

/// <summary>
///     Lowers the alpha of the paint the element is composited with, in pre-processing
/// </summary>
public class OpacityTestEffect : LayerEffect<OpacityTestEffectProperties>
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
        paint.Color = paint.Color.WithAlpha((byte) (paint.Color.Alpha * Properties.Opacity.CurrentValue / 100f));
    }

    public override void PostProcess(SKCanvas canvas, SKRect renderBounds, SKPaint paint)
    {
    }
}
