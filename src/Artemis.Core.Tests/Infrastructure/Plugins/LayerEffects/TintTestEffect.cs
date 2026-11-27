using Artemis.Core.LayerEffects;
using SkiaSharp;

namespace Artemis.Core.Tests.Infrastructure.Plugins.LayerEffects;

/// <summary>
///     Multiplies whatever was rendered with a color, in post-processing
/// </summary>
public class TintTestEffect : LayerEffect<TintTestEffectProperties>
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
    }

    public override void PostProcess(SKCanvas canvas, SKRect renderBounds, SKPaint paint)
    {
        using SKPaint tint = new() {Color = Properties.Color.CurrentValue, BlendMode = SKBlendMode.Modulate};
        canvas.DrawRect(renderBounds, tint);
    }
}
