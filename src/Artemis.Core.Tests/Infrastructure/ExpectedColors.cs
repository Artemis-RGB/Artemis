using SkiaSharp;

namespace Artemis.Core.Tests.Infrastructure;

/// <summary>
///     Helpers to describe expected colors by what produced them rather than by their channel values.
/// </summary>
public static class ExpectedColors
{
    /// <summary>
    ///     Returns the color an LED shows when <paramref name="color" /> is drawn at the given opacity over an opaque
    ///     background. The canvas is cleared to black before each frame, so that is the default background.
    /// </summary>
    /// <param name="color">The color being drawn</param>
    /// <param name="opacity">The opacity in percent, like the opacity properties of layers and effects</param>
    /// <param name="over">The opaque color underneath, defaults to black</param>
    public static SKColor AtOpacity(this SKColor color, float opacity, SKColor? over = null)
    {
        SKColor background = over ?? SKColors.Black;
        float alpha = opacity / 100f;
        return new SKColor(
            Blend(color.Red, background.Red, alpha),
            Blend(color.Green, background.Green, alpha),
            Blend(color.Blue, background.Blue, alpha)
        );
    }

    private static byte Blend(byte top, byte bottom, float alpha)
    {
        return (byte) MathF.Round(top * alpha + bottom * (1 - alpha));
    }
}
