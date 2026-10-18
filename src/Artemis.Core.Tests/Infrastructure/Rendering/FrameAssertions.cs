using SkiaSharp;

namespace Artemis.Core.Tests.Infrastructure.Rendering;

public static class FrameAssertions
{
    /// <summary>
    ///     Asserts that, compared to <paramref name="before" />, the affected LEDs changed according to
    ///     <paramref name="expected" /> and every other LED (apart from the ignored ones) stayed the same.
    ///     <para>Also asserts that most affected LEDs actually changed, to guard against a vacuous pass.</para>
    /// </summary>
    public static void ShouldOnlyDifferFrom(this RenderFrame after, RenderFrame before, IReadOnlyCollection<ArtemisLed> affected, Func<SKColor, SKColor> expected,
        IReadOnlyCollection<ArtemisLed>? ignored = null, int tolerance = 3)
    {
        affected.Count(l => before.LedColors[l] != after.LedColors[l]).ShouldBeGreaterThan(affected.Count / 2, "Most of the affected LEDs should have changed");

        foreach ((ArtemisLed led, SKColor beforeColor) in before.LedColors)
        {
            if (ignored != null && ignored.Contains(led))
                continue;

            SKColor expectedColor = affected.Contains(led) ? expected(beforeColor) : beforeColor;
            after.LedColors[led].ShouldBeColor(expectedColor, tolerance, $"{led.RgbLed.Id} was {beforeColor}");
        }
    }
}
