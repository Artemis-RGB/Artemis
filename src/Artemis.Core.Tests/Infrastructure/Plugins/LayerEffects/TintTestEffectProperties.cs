using SkiaSharp;

namespace Artemis.Core.Tests.Infrastructure.Plugins.LayerEffects;

public class TintTestEffectProperties : LayerEffectPropertyGroup
{
    public SKColorLayerProperty Color { get; set; } = null!;

    protected override void PopulateDefaults()
    {
        Color.DefaultValue = new SKColor(255, 0, 0);
    }

    protected override void EnableProperties()
    {
    }

    protected override void DisableProperties()
    {
    }
}
