using Artemis.Core.Tests.Infrastructure;
using Artemis.Core.Tests.Infrastructure.Plugins.DeviceProviders;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;
using Artemis.Core.Tests.Infrastructure.Rendering;
using SkiaSharp;

namespace Artemis.Core.Tests.Rendering;

/// <summary>
///     LEDs smaller than a canvas pixel, like those of LED strips at low render scales, must remain individually
///     addressable. Each test creates its own harness since the render scale is fixed per harness.
/// </summary>
public class SmallLedTests
{
    public static TheoryData<float> RenderScales => new(1f, 0.5f, 0.25f);

    [Theory]
    [MemberData(nameof(RenderScales))]
    public void SmallLeds_AreIndividuallyAddressableThroughALayer(float renderScale)
    {
        using RenderHarness harness = new(renderScale);
        ArtemisDevice strip = harness.AddDevice(TestLayouts.SmallLedStrip);
        Profile profile = harness.CreateProfile();
        (_, PerLedTestBrush brush) = harness.AddLayer<PerLedTestBrush>(profile.GetRootFolder(), strip.Leds);
        brush.ColorSelector = (led, _) => ColorFor(strip, led);

        using RenderFrame frame = harness.Render(profile, TimeSpan.FromSeconds(0.1));

        foreach (ArtemisLed led in strip.Leds)
            frame.LedColors[led].ShouldBeColor(ColorFor(strip, led), 2, $"{led.RgbLed.Id} {led.AbsoluteRectangle}");
    }

    [Theory]
    [MemberData(nameof(RenderScales))]
    public void SmallLeds_ReceiveASolidLayer(float renderScale)
    {
        using RenderHarness harness = new(renderScale);
        ArtemisDevice strip = harness.AddDevice(TestLayouts.SmallLedStrip);
        Profile profile = harness.CreateProfile();
        harness.AddLayer<SolidColorTestBrush>(profile.GetRootFolder(), strip.Leds);

        using RenderFrame frame = harness.Render(profile, TimeSpan.FromSeconds(0.1));

        foreach (ArtemisLed led in strip.Leds)
            frame.LedColors[led].ShouldBeColor(SKColors.Red, 2, $"{led.RgbLed.Id} {led.AbsoluteRectangle}");
    }

    private static SKColor ColorFor(ArtemisDevice strip, ArtemisLed led)
    {
        // Alternate between two colors so neighbouring LEDs can't pass by sampling each other
        return strip.Leds.IndexOf(led) % 2 == 0 ? SKColors.Red : SKColors.Blue;
    }
}
