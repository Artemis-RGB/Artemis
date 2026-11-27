using Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;
using Artemis.Core.Tests.Infrastructure.Rendering;
using Artemis.Core.Tests.Infrastructure.VisualScripting;
using RGB.NET.Core;

namespace Artemis.Core.Tests.Profiles;

/// <summary>
///     Checks that work done for every property or node on every frame doesn't allocate.
/// </summary>
public class PerFrameAllocationTests : RenderTestBase
{
    [Fact]
    public void LayerPropertyUpdate_WithoutKeyframes_DoesNotAllocate()
    {
        FloatLayerProperty opacity = CreateOpacityProperty();
        Timeline timeline = new();

        AllocatedBytes(() => opacity.Update(timeline)).ShouldBe(0);
    }

    [Fact]
    public void LayerPropertyUpdate_WithKeyframes_DoesNotAllocate()
    {
        FloatLayerProperty opacity = CreateOpacityProperty();
        opacity.AddKeyframe(new LayerPropertyKeyframe<float>(0, TimeSpan.Zero, Easings.Functions.Linear, opacity));
        opacity.AddKeyframe(new LayerPropertyKeyframe<float>(100, TimeSpan.FromSeconds(1), Easings.Functions.Linear, opacity));
        Timeline timeline = new();
        timeline.Override(TimeSpan.FromSeconds(0.5), false);

        AllocatedBytes(() => opacity.Update(timeline)).ShouldBe(0);
    }

    [Fact]
    public void NodeTryEvaluate_DoesNotAllocate()
    {
        TestNode node = new();

        AllocatedBytes(node.TryEvaluate).ShouldBe(0);
    }

    [Fact]
    public void ReadingPinsAndConnections_DoesNotAllocate()
    {
        TestNode source = new("Source");
        TestNode target = new("Target");
        OutputPin<Numeric> output = source.CreateOutputPin<Numeric>();
        InputPin<Numeric> input = target.CreateInputPin<Numeric>();
        PinCollection collection = target.CreateInputPinCollection<Numeric>();
        input.ConnectTo(output);

        int read = 0;
        AllocatedBytes(() =>
        {
            if (input.ConnectedTo[0] == output)
                read += input.ConnectedTo.Count + target.Pins.Count + target.PinCollections.Count + collection.Pins.Count;
        }).ShouldBe(0);
        read.ShouldBeGreaterThan(0);
    }

    private FloatLayerProperty CreateOpacityProperty()
    {
        Profile profile = Harness.CreateProfile();
        (Layer layer, _) = Harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), [Led(LedId.Keyboard_A)]);
        return layer.Transform.Opacity;
    }

    private static long AllocatedBytes(Action action)
    {
        // Warm up first so JIT compilation and lazy initialization don't count
        for (int i = 0; i < 10; i++)
            action();

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 100; i++)
            action();
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }
}
