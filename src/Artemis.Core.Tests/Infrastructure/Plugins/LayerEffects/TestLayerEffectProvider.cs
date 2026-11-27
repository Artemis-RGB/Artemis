using Artemis.Core.LayerEffects;

namespace Artemis.Core.Tests.Infrastructure.Plugins.LayerEffects;

/// <summary>
///     Provides deterministic effects for render tests. The real effects live in the Artemis.Plugins repository.
/// </summary>
public class TestLayerEffectProvider : LayerEffectProvider
{
    public override void Enable()
    {
        RegisterLayerEffectDescriptor<OpacityTestEffect>("Opacity", "Pre-processes the paint by lowering its alpha", "Opacity");
        RegisterLayerEffectDescriptor<ChannelSwapTestEffect>("Channel swap", "Pre-processes the paint with a filter that swaps red and blue", "SwapHorizontal");
        RegisterLayerEffectDescriptor<TintTestEffect>("Tint", "Post-processes the canvas by multiplying it with a color", "Palette");
        RegisterLayerEffectDescriptor<GlowTestEffect>("Glow", "Pre-processes the paint with a filter that grows the content outwards", "Brightness7");
    }

    public override void Disable()
    {
    }
}
