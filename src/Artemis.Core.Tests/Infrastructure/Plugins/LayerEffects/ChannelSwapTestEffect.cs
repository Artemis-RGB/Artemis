using Artemis.Core.LayerEffects;
using SkiaSharp;

namespace Artemis.Core.Tests.Infrastructure.Plugins.LayerEffects;

/// <summary>
///     Gives the paint the element is composited with a color filter that swaps the red and blue channels, in pre-processing
/// </summary>
public class ChannelSwapTestEffect : LayerEffect<ChannelSwapTestEffectProperties>
{
    private static readonly float[] SwapRedBlue =
    [
        0, 0, 1, 0, 0,
        0, 1, 0, 0, 0,
        1, 0, 0, 0, 0,
        0, 0, 0, 1, 0
    ];

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
        SKColorFilter swap = SKColorFilter.CreateColorMatrix(SwapRedBlue);
        paint.ColorFilter = paint.ColorFilter == null ? swap : SKColorFilter.CreateCompose(swap, paint.ColorFilter);
    }

    public override void PostProcess(SKCanvas canvas, SKRect renderBounds, SKPaint paint)
    {
    }
}
