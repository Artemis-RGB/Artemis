using SkiaSharp;

namespace Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;

public class GradientTestBrushProperties : LayerPropertyGroup
{
    public SKColorLayerProperty Left { get; set; } = null!;
    public SKColorLayerProperty Right { get; set; } = null!;

    protected override void PopulateDefaults()
    {
        Left.DefaultValue = SKColors.Black;
        Right.DefaultValue = SKColors.White;
    }

    protected override void EnableProperties()
    {
    }

    protected override void DisableProperties()
    {
    }
}
