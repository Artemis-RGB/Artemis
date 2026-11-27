using SkiaSharp;

namespace Artemis.Core.Tests.Infrastructure;

public static class ColorAssertions
{
    /// <summary>
    ///     Asserts the color matches per channel within the given tolerance, rendering results vary slightly between platforms
    ///     due to anti-aliasing and the render scale.
    /// </summary>
    public static void ShouldBeColor(this SKColor actual, SKColor expected, int tolerance = 2, string? context = null)
    {
        bool matches = Math.Abs(actual.Red - expected.Red) <= tolerance &&
                       Math.Abs(actual.Green - expected.Green) <= tolerance &&
                       Math.Abs(actual.Blue - expected.Blue) <= tolerance &&
                       Math.Abs(actual.Alpha - expected.Alpha) <= tolerance;

        matches.ShouldBeTrue($"Expected {expected} (±{tolerance}) but got {actual}{(context == null ? "" : $" ({context})")}");
    }
}
