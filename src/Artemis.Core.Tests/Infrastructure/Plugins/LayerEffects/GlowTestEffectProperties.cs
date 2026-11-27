namespace Artemis.Core.Tests.Infrastructure.Plugins.LayerEffects;

public class GlowTestEffectProperties : LayerEffectPropertyGroup
{
    public FloatLayerProperty Radius { get; set; } = null!;

    protected override void PopulateDefaults()
    {
        // Keys are about 18 units wide with a 1 unit gap, 10 units reaches well into the neighbouring key
        Radius.DefaultValue = 10;
    }

    protected override void EnableProperties()
    {
    }

    protected override void DisableProperties()
    {
    }
}
