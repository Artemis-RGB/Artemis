using Artemis.Core.LayerBrushes;
using SkiaSharp;

namespace Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;

/// <summary>
///     Fills the layer with a horizontal gradient, useful to see the direction of transformations.
/// </summary>
public class GradientTestBrush : LayerBrush<GradientTestBrushProperties>
{
    public override void EnableLayerBrush()
    {
    }

    public override void DisableLayerBrush()
    {
    }

    public override void Update(double deltaTime)
    {
    }

    public override void Render(SKCanvas canvas, SKRect bounds, SKPaint paint)
    {
        using SKShader shader = SKShader.CreateLinearGradient(
            new SKPoint(bounds.Left, bounds.MidY),
            new SKPoint(bounds.Right, bounds.MidY),
            [Properties.Left.CurrentValue, Properties.Right.CurrentValue],
            SKShaderTileMode.Clamp
        );

        paint.Shader = shader;
        canvas.DrawRect(bounds, paint);
        paint.Shader = null;
    }
}
