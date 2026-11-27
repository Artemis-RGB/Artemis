using Artemis.Core.LayerBrushes;

namespace Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;

/// <summary>
///     Provides deterministic brushes for render tests. The real brushes live in the Artemis.Plugins repository.
/// </summary>
public class TestLayerBrushProvider : LayerBrushProvider
{
    public override void Enable()
    {
        RegisterLayerBrushDescriptor<SolidColorTestBrush>("Solid color", "Fills the layer with a single color", "Brush");
        RegisterLayerBrushDescriptor<GradientTestBrush>("Gradient", "Fills the layer with a left to right gradient", "Gradient");
        RegisterLayerBrushDescriptor<PerLedTestBrush>("Per LED", "Lets the test pick a color for each LED", "Led");
    }

    public override void Disable()
    {
    }
}
