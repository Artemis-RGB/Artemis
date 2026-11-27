namespace Artemis.Core.Tests.Infrastructure.Plugins.LayerEffects;

public class OpacityTestEffectProperties : LayerEffectPropertyGroup
{
    public FloatLayerProperty Opacity { get; set; } = null!;

    protected override void PopulateDefaults()
    {
        Opacity.DefaultValue = 50;
    }

    protected override void EnableProperties()
    {
    }

    protected override void DisableProperties()
    {
    }
}
