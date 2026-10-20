using Artemis.Core.LayerBrushes;
using SkiaSharp;

namespace Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;

public class SolidColorTestBrush : LayerBrush<SolidColorTestBrushProperties>
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
        // Like the real solid brush this draws with a shader, which keeps the alpha of the paint it is given
        using SKShader shader = SKShader.CreateColor(Properties.Color.CurrentValue);
        paint.Shader = shader;
        canvas.DrawRect(bounds, paint);
        paint.Shader = null;
    }
}
