using SkiaSharp;

namespace Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;

public class SolidColorTestBrushProperties : LayerPropertyGroup
{
    public SKColorLayerProperty Color { get; set; } = null!;

    protected override void PopulateDefaults()
    {
        Color.DefaultValue = SKColors.Red;
    }

    protected override void EnableProperties()
    {
    }

    protected override void DisableProperties()
    {
    }
}
