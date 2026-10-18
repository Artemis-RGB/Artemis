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
        // Like the real brushes this overrides the paint color, the layer applies its opacity when compositing
        paint.Color = Properties.Color.CurrentValue;
        canvas.DrawRect(bounds, paint);
    }
}
