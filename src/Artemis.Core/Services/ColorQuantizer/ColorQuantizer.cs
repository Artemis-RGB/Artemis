using System;
using System.Collections.Generic;
using SkiaSharp;

namespace Artemis.Core.Services;

/// <summary>
/// A service providing a pallette of colors in a bitmap based on vibrant.js
/// </summary>
public static class ColorQuantizer
{
    #region Properties & Fields

    /// <summary>
    /// Target luma for dark color variants. (see <see cref="ColorType"/>)
    /// </summary>
    public static float TargetDarkLuma { get; set; } = 0.26f;

    /// <summary>
    /// Maximum luma for dark color variants. (see <see cref="ColorType"/>)
    /// </summary>
    public static float MaxDarkLuma { get; set; } = 0.45f;

    /// <summary>
    /// Minimum luma for light color variants. (see <see cref="ColorType"/>)
    /// </summary>
    public static float MinLightLuma { get; set; } = 0.55f;

    /// <summary>
    /// Target luma for light color variants. (see <see cref="ColorType"/>)
    /// </summary>
    public static float TargetLightLuma { get; set; } = 0.74f;

    /// <summary>
    /// Minimum luma for normal color variants. (see <see cref="ColorType"/>)
    /// </summary>
    public static float MinNormalLuma { get; set; } = 0.3f;

    /// <summary>
    /// Target luma for normal color variants. (see <see cref="ColorType"/>)
    /// </summary>
    public static float TargetNormalLuma { get; set; } = 0.5f;

    /// <summary>
    /// Maximum luma for normal color variants. (see <see cref="ColorType"/>)
    /// </summary>
    public static float MaxNormalLuma { get; set; } = 0.7f;

    /// <summary>
    /// Target saturation for muted color variants. (see <see cref="ColorType"/>)
    /// </summary>
    public static float TargetMutesSaturation { get; set; } = 0.3f;

    /// <summary>
    /// Maximum saturation for muted color variants. (see <see cref="ColorType"/>)
    /// </summary>
    public static float MaxMutesSaturation { get; set; } = 0.3f;

    /// <summary>
    /// Target saturation for vibrant color variants. (see <see cref="ColorType"/>)
    /// </summary>
    public static float TargetVibrantSaturation { get; set; } = 1.0f;

    /// <summary>
    /// Minimum saturation for vibrant color variants. (see <see cref="ColorType"/>)
    /// </summary>
    public static float MinVibrantSaturation { get; set; } = 0.35f;

    /// <summary>
    /// Weight of the saturation value.
    /// </summary>
    public static float WeightSaturation { get; set; } = 3f;

    /// <summary>
    /// Weight of the luma value.
    /// </summary>
    public static float WeightLuma { get; set; } = 5f;

    #endregion
    
    #region Methods

    /// <summary>
    /// Reduces an <see cref="SKImage"/> to a given amount of relevant colors. Based on the Median Cut algorithm.
    /// </summary>
    /// <param name="image">The image to quantize.</param>
    /// <param name="amount">The number of colors that should be calculated. Must be a power of two.</param>
    /// <returns>The quantized colors.</returns>
    public static SKColor[] Quantize(in SKImage image, int amount = 32)
    {
        using SKBitmap bitmap = SKBitmap.FromImage(image);
        return Quantize(bitmap.Pixels, amount);
    }

    /// <summary>
    /// Reduces an <see cref="Span{SKColor}"/> to a given amount of relevant colors. Based on the Median Cut algorithm.
    /// </summary>
    /// <param name="colors">The colors to quantize.</param>
    /// <param name="amount">The number of colors that should be calculated. Must be a power of two.</param>
    /// <returns>The quantized colors.</returns>
    public static SKColor[] Quantize(in Span<SKColor> colors, int amount = 32)
    {
        if ((amount & (amount - 1)) != 0)
            throw new ArgumentException("Must be power of two", nameof(amount));

        Queue<ColorCube> cubes = new(amount);
        cubes.Enqueue(new ColorCube(colors, 0, colors.Length, SortTarget.None));

        while (cubes.Count < amount)
        {
            ColorCube cube = cubes.Dequeue();

            if (cube.TrySplit(colors, out ColorCube? a, out ColorCube? b))
            {
                cubes.Enqueue(a);
                cubes.Enqueue(b);
            }
        }

        SKColor[] result = new SKColor[cubes.Count];
        int i = 0;
        foreach (ColorCube colorCube in cubes)
            result[i++] = colorCube.GetAverageColor(colors);

        return result;
    }

    /// <summary>
    /// Finds colors with certain characteristics in a given <see cref="IEnumerable{SKColor}"/>.<para />
    /// Vibrant variants are more saturated, while Muted colors are less.<para />
    /// Light and Dark colors have higher and lower lightness values, respectively.
    /// </summary>
    /// <param name="colors">The colors to find the variations in</param>
    /// <param name="type">Which type of color to find</param>
    /// <param name="ignoreLimits">Ignore hard limits on whether a color is considered for each category. Result may be <see cref="SKColor.Empty"/> if this is false</param>
    /// <returns>The color found</returns>
    public static SKColor FindColorVariation(IEnumerable<SKColor> colors, ColorType type, bool ignoreLimits = false)
    {
        SKColor bestColor = SKColor.Empty;
        float bestColorScore = 0;

        foreach (SKColor color in colors)
        {
            float score = GetScore(color, type, ignoreLimits);
            if (score > bestColorScore)
            {
                bestColorScore = score;
                bestColor = color;
            }
        }

        return bestColor;
    }

    /// <summary>
    /// Finds all the color variations available and returns a struct containing them all.
    /// </summary>
    /// <param name="colors">The colors to find the variations in</param>
    /// <param name="ignoreLimits">Ignore hard limits on whether a color is considered for each category. Some colors may be <see cref="SKColor.Empty"/> if this is false</param>
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
    /// Quantizes the image, finds all the color variations available and returns a struct containing them all.
    /// </summary>
    /// <param name="image">The image to quantize.</param>
    /// <param name="amount">The number of colors that should be calculated. Must be a power of two.</param>
    /// <param name="ignoreLimits">Ignore hard limits on whether a color is considered for each category. Some colors may be <see cref="SKColor.Empty"/> if this is false</param>
    /// <returns></returns>
    public static ColorSwatch GetColorVariations(in SKImage image, int amount = 32, bool ignoreLimits = false) => FindAllColorVariations(Quantize(image, amount), ignoreLimits);

    /// <summary>
    /// Quantizes the colors, finds all the color variations available and returns a struct containing them all.
    /// </summary>
    /// <param name="colors">The colors to quantize.</param>
    /// <param name="amount">The number of colors that should be calculated. Must be a power of two.</param>
    /// <param name="ignoreLimits">Ignore hard limits on whether a color is considered for each category. Some colors may be <see cref="SKColor.Empty"/> if this is false</param>
    /// <returns></returns>
    public static ColorSwatch GetColorVariations(in Span<SKColor> colors, int amount = 32, bool ignoreLimits = false) => FindAllColorVariations(Quantize(colors, amount), ignoreLimits);

    private static float GetScore(SKColor color, ColorType type, bool ignoreLimits = false)
    {
        static float InvertDiff(float value, float target) => 1 - Math.Abs(value - target);

        color.ToHsl(out float _, out float saturation, out float luma);
        saturation /= 100f;
        luma /= 100f;

        if (!ignoreLimits && ((saturation <= GetMinSaturation(type)) || (saturation >= GetMaxSaturation(type)) || (luma <= GetMinLuma(type)) || (luma >= GetMaxLuma(type))))
        {
            //if either saturation or luma falls outside the min-max, return the
            //lowest score possible unless we're ignoring these limits.
            return float.MinValue;
        }

        float totalValue = (InvertDiff(saturation, GetTargetSaturation(type)) * WeightSaturation) + (InvertDiff(luma, GetTargetLuma(type)) * WeightLuma);
        float totalWeight = WeightSaturation + WeightLuma;

        return totalValue / totalWeight;
    }

    private static float GetTargetLuma(ColorType colorType) => colorType switch
    {
        ColorType.Vibrant => TargetNormalLuma,
        ColorType.LightVibrant => TargetLightLuma,
        ColorType.DarkVibrant => TargetDarkLuma,
        ColorType.Muted => TargetNormalLuma,
        ColorType.LightMuted => TargetLightLuma,
        ColorType.DarkMuted => TargetDarkLuma,
        _ => throw new ArgumentException(nameof(colorType))
    };

    private static float GetMinLuma(ColorType colorType) => colorType switch
    {
        ColorType.Vibrant => MinNormalLuma,
        ColorType.LightVibrant => MinLightLuma,
        ColorType.DarkVibrant => 0f,
        ColorType.Muted => MinNormalLuma,
        ColorType.LightMuted => MinLightLuma,
        ColorType.DarkMuted => 0,
        _ => throw new ArgumentException(nameof(colorType))
    };

    private static float GetMaxLuma(ColorType colorType) => colorType switch
    {
        ColorType.Vibrant => MaxNormalLuma,
        ColorType.LightVibrant => 1f,
        ColorType.DarkVibrant => MaxDarkLuma,
        ColorType.Muted => MaxNormalLuma,
        ColorType.LightMuted => 1f,
        ColorType.DarkMuted => MaxDarkLuma,
        _ => throw new ArgumentException(nameof(colorType))
    };

    private static float GetTargetSaturation(ColorType colorType) => colorType switch
    {
        ColorType.Vibrant => TargetVibrantSaturation,
        ColorType.LightVibrant => TargetVibrantSaturation,
        ColorType.DarkVibrant => TargetVibrantSaturation,
        ColorType.Muted => TargetMutesSaturation,
        ColorType.LightMuted => TargetMutesSaturation,
        ColorType.DarkMuted => TargetMutesSaturation,
        _ => throw new ArgumentException(nameof(colorType))
    };

    private static float GetMinSaturation(ColorType colorType) => colorType switch
    {
        ColorType.Vibrant => MinVibrantSaturation,
        ColorType.LightVibrant => MinVibrantSaturation,
        ColorType.DarkVibrant => MinVibrantSaturation,
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
        ColorType.Muted => MaxMutesSaturation,
        ColorType.LightMuted => MaxMutesSaturation,
        ColorType.DarkMuted => MaxMutesSaturation,
        _ => throw new ArgumentException(nameof(colorType))
    };

    #endregion
}