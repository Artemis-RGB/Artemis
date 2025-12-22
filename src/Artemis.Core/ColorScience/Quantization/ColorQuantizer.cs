using HPPH;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Artemis.Core.ColorScience;

/// <summary>
/// Helper class for color quantization.
/// </summary>
public static class ColorQuantizer
{
    /// <inheritdoc cref="Quantize(Span{SKColor}, int)"/>
    [Obsolete("Use Quantize(Span<SKColor> colors, int amount) in-parameter instead")]
    public static SKColor[] Quantize(in Span<SKColor> colors, int amount)
    {
        return Quantize(colors, amount);
    }

    /// <inheritdoc cref="QuantizeSplit(Span{SKColor}, int)"/>
    [Obsolete("Use QuantizeSplit(Span<SKColor> colors, int splits) without the in-parameter instead")]
    public static SKColor[] QuantizeSplit(in Span<SKColor> colors, int splits)
    {
        return QuantizeSplit(colors, splits);
    }
    
    /// <summary>
    /// Quantizes a span of colors into the desired amount of representative colors.
    /// </summary>
    /// <param name="colors">The colors to quantize</param>
    /// <param name="amount">How many colors to return. Must be a power of two.</param>
    /// <returns><paramref name="amount"/> colors.</returns>
    public static SKColor[] Quantize(Span<SKColor> colors, int amount)
    {
        if (!BitOperations.IsPow2(amount))
            throw new ArgumentException("Must be power of two", nameof(amount));

        int splits = BitOperations.Log2((uint)amount);
        return QuantizeSplit(colors, splits);
    }
   
    /// <summary>
    /// Quantizes a span of colors, splitting the average <paramref name="splits"/> number of times.
    /// </summary>
    /// <param name="colors">The colors to quantize</param>
    /// <param name="splits">How many splits to execute. Each split doubles the number of colors returned.</param>
    /// <returns>Up to (2 ^ <paramref name="splits"/>) number of colors.</returns>
    public static SKColor[] QuantizeSplit(Span<SKColor> colors, int splits)
    {
        if (colors.Length < (1 << splits)) throw new ArgumentException($"The color array must at least contain ({(1 << splits)}) to perform {splits} splits.");

        // DarthAffe 22.07.2024: This is not ideal as it allocates an additional array, but i don't see a way to get SKColors out here
        return MemoryMarshal.Cast<ColorBGRA, SKColor>(MemoryMarshal.Cast<SKColor, ColorBGRA>(colors).CreateSimpleColorPalette(1 << splits)).ToArray();
    }

    /// <summary>
    ///     Finds colors with certain characteristics in a given <see cref="IEnumerable{SKColor}" />.
    ///     <para />
    ///     Vibrant variants are more saturated, while Muted colors are less.
    ///     <para />
    ///     Light and Dark colors have higher and lower lightness values, respectively.
    /// </summary>
    /// <param name="colors">The colors to find the variations in</param>
    /// <param name="type">Which type of color to find</param>
    /// <param name="ignoreLimits">
    ///     Ignore hard limits on whether a color is considered for each category. Result may be
    ///     <see cref="SKColor.Empty" /> if this is false
    /// </param>
    /// <returns>The color found</returns>
    public static SKColor FindColorVariation(IEnumerable<SKColor> colors, ColorType type, bool ignoreLimits = false)
    {
        float bestColorScore = 0;
        SKColor bestColor = SKColor.Empty;

        foreach (SKColor clr in colors)
        {
            float score = GetScore(clr, type, ignoreLimits);
            if (score > bestColorScore)
            {
                bestColorScore = score;
                bestColor = clr;
            }
        }

        return bestColor;
    }

    /// <summary>
    ///     Finds all the color variations available and returns a struct containing them all.
    /// </summary>
    /// <param name="colors">The colors to find the variations in</param>
    /// <param name="ignoreLimits">
    ///     Ignore hard limits on whether a color is considered for each category. Some colors may be
    ///     <see cref="SKColor.Empty" /> if this is false
    /// </param>
    /// <returns>A swatch containing all color variations</returns>
    public static ColorSwatch FindAllColorVariations(IEnumerable<SKColor> colors, bool ignoreLimits = false)
    {
        SKColor bestVibrantColor = SKColor.Empty;
        SKColor bestLightVibrantColor = SKColor.Empty;
        SKColor bestDarkVibrantColor = SKColor.Empty;
        SKColor bestMutedColor = SKColor.Empty;
        SKColor bestLightMutedColor = SKColor.Empty;
        SKColor bestDarkMutedColor = SKColor.Empty;
        float bestVibrantScore = float.MinValue;
        float bestLightVibrantScore = float.MinValue;
        float bestDarkVibrantScore = float.MinValue;
        float bestMutedScore = float.MinValue;
        float bestLightMutedScore = float.MinValue;
        float bestDarkMutedScore = float.MinValue;

        //ugly but at least we only loop through the enumerable once ¯\_(ツ)_/¯
        foreach (SKColor color in colors)
        {
            static void SetIfBetterScore(ref float bestScore, ref SKColor bestColor, SKColor newColor, ColorType type, bool ignoreLimits)
            {
                float newScore = GetScore(newColor, type, ignoreLimits);
                if (newScore > bestScore)
                {
                    bestScore = newScore;
                    bestColor = newColor;
                }
            }

            SetIfBetterScore(ref bestVibrantScore, ref bestVibrantColor, color, ColorType.Vibrant, ignoreLimits);
            SetIfBetterScore(ref bestLightVibrantScore, ref bestLightVibrantColor, color, ColorType.LightVibrant, ignoreLimits);
            SetIfBetterScore(ref bestDarkVibrantScore, ref bestDarkVibrantColor, color, ColorType.DarkVibrant, ignoreLimits);
            SetIfBetterScore(ref bestMutedScore, ref bestMutedColor, color, ColorType.Muted, ignoreLimits);
            SetIfBetterScore(ref bestLightMutedScore, ref bestLightMutedColor, color, ColorType.LightMuted, ignoreLimits);
            SetIfBetterScore(ref bestDarkMutedScore, ref bestDarkMutedColor, color, ColorType.DarkMuted, ignoreLimits);
        }

        return new ColorSwatch
        {
            Vibrant = bestVibrantColor,
            LightVibrant = bestLightVibrantColor,
            DarkVibrant = bestDarkVibrantColor,
            Muted = bestMutedColor,
            LightMuted = bestLightMutedColor,
            DarkMuted = bestDarkMutedColor,
        };
    }

    /// <summary>
    /// Gets a gradient from a given image.
    /// </summary>
    /// <param name="bitmap">The image to process</param>
    public static ColorGradient GetGradientFromImage(SKBitmap bitmap)
    {
        SKColor[] colors = QuantizeSplit(bitmap.Pixels, 8);
        ColorSwatch swatch = FindAllColorVariations(colors);
        SKColor[] swatchArray =
        [
            swatch.Muted,
            swatch.Vibrant,
            swatch.DarkMuted,
            swatch.DarkVibrant,
            swatch.LightMuted,
            swatch.LightVibrant
        ];

        ColorSorter.Sort(swatchArray, SKColors.Black);

        ColorGradient gradient = [];

        for (int i = 0; i < swatchArray.Length; i++)
            gradient.Add(new ColorGradientStop(swatchArray[i], (float)i / (swatchArray.Length - 1)));

        return gradient;
    }

    private static float GetScore(SKColor color, ColorType type, bool ignoreLimits = false)
    {
        static float InvertDiff(float value, float target)
        {
            return 1 - Math.Abs(value - target);
        }

        color.ToHsl(out float _, out float saturation, out float luma);
        saturation /= 100f;
        luma /= 100f;

        if (!ignoreLimits && ((saturation <= GetMinSaturation(type)) || (saturation >= GetMaxSaturation(type)) || (luma <= GetMinLuma(type)) || (luma >= GetMaxLuma(type))))
        {
            //if either saturation or luma falls outside the min-max, return the
            //lowest score possible unless we're ignoring these limits.
            return float.MinValue;
        }

        float totalValue = (InvertDiff(saturation, GetTargetSaturation(type)) * WEIGHT_SATURATION) + (InvertDiff(luma, GetTargetLuma(type)) * WEIGHT_LUMA);

        const float TOTAL_WEIGHT = WEIGHT_SATURATION + WEIGHT_LUMA;

        return totalValue / TOTAL_WEIGHT;
    }

    #region Constants

    private const float TARGET_DARK_LUMA = 0.26f;
    private const float MAX_DARK_LUMA = 0.45f;
    private const float MIN_LIGHT_LUMA = 0.55f;
    private const float TARGET_LIGHT_LUMA = 0.74f;
    private const float MIN_NORMAL_LUMA = 0.3f;
    private const float TARGET_NORMAL_LUMA = 0.5f;
    private const float MAX_NORMAL_LUMA = 0.7f;
    private const float TARGET_MUTES_SATURATION = 0.3f;
    private const float MAX_MUTES_SATURATION = 0.3f;
    private const float TARGET_VIBRANT_SATURATION = 1.0f;
    private const float MIN_VIBRANT_SATURATION = 0.35f;
    private const float WEIGHT_SATURATION = 3f;
    private const float WEIGHT_LUMA = 5f;

    private static float GetTargetLuma(ColorType colorType) => colorType switch
    {
        ColorType.Vibrant => TARGET_NORMAL_LUMA,
        ColorType.LightVibrant => TARGET_LIGHT_LUMA,
        ColorType.DarkVibrant => TARGET_DARK_LUMA,
        ColorType.Muted => TARGET_NORMAL_LUMA,
        ColorType.LightMuted => TARGET_LIGHT_LUMA,
        ColorType.DarkMuted => TARGET_DARK_LUMA,
        _ => throw new ArgumentException(nameof(colorType))
    };

    private static float GetMinLuma(ColorType colorType) => colorType switch
    {
        ColorType.Vibrant => MIN_NORMAL_LUMA,
        ColorType.LightVibrant => MIN_LIGHT_LUMA,
        ColorType.DarkVibrant => 0f,
        ColorType.Muted => MIN_NORMAL_LUMA,
        ColorType.LightMuted => MIN_LIGHT_LUMA,
        ColorType.DarkMuted => 0,
        _ => throw new ArgumentException(nameof(colorType))
    };

    private static float GetMaxLuma(ColorType colorType) => colorType switch
    {
        ColorType.Vibrant => MAX_NORMAL_LUMA,
        ColorType.LightVibrant => 1f,
        ColorType.DarkVibrant => MAX_DARK_LUMA,
        ColorType.Muted => MAX_NORMAL_LUMA,
        ColorType.LightMuted => 1f,
        ColorType.DarkMuted => MAX_DARK_LUMA,
        _ => throw new ArgumentException(nameof(colorType))
    };

    private static float GetTargetSaturation(ColorType colorType) => colorType switch
    {
        ColorType.Vibrant => TARGET_VIBRANT_SATURATION,
        ColorType.LightVibrant => TARGET_VIBRANT_SATURATION,
        ColorType.DarkVibrant => TARGET_VIBRANT_SATURATION,
        ColorType.Muted => TARGET_MUTES_SATURATION,
        ColorType.LightMuted => TARGET_MUTES_SATURATION,
        ColorType.DarkMuted => TARGET_MUTES_SATURATION,
        _ => throw new ArgumentException(nameof(colorType))
    };

    private static float GetMinSaturation(ColorType colorType) => colorType switch
    {
        ColorType.Vibrant => MIN_VIBRANT_SATURATION,
        ColorType.LightVibrant => MIN_VIBRANT_SATURATION,
        ColorType.DarkVibrant => MIN_VIBRANT_SATURATION,
        ColorType.Muted => 0,
        ColorType.LightMuted => 0,
        ColorType.DarkMuted => 0,
        _ => throw new ArgumentException(nameof(colorType))
    };

    private static float GetMaxSaturation(ColorType colorType) => colorType switch
    {
        ColorType.Vibrant => 1f,
        ColorType.LightVibrant => 1f,
        ColorType.DarkVibrant => 1f,
        ColorType.Muted => MAX_MUTES_SATURATION,
        ColorType.LightMuted => MAX_MUTES_SATURATION,
        ColorType.DarkMuted => MAX_MUTES_SATURATION,
        _ => throw new ArgumentException(nameof(colorType))
    };

    #endregion
}
