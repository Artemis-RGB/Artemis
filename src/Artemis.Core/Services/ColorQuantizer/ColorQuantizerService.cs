using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace Artemis.Core.Services;

/// <inheritdoc />
internal class ColorQuantizerService : IColorQuantizerService
{
    private static float GetScore(SKColor color, ColorType type, bool ignoreLimits = false)
    {
        static float InvertDiff(float value, float target)
        {
            return 1 - Math.Abs(value - target);
        }

        color.ToHsl(out float _, out float saturation, out float luma);
        saturation /= 100f;
        luma /= 100f;

        if (!ignoreLimits &&
            (saturation <= GetMinSaturation(type) || saturation >= GetMaxSaturation(type)
                                                  || luma <= GetMinLuma(type) || luma >= GetMaxLuma(type)))
            //if either saturation or luma falls outside the min-max, return the
            //lowest score possible unless we're ignoring these limits.
            return float.MinValue;

        float totalValue = InvertDiff(saturation, GetTargetSaturation(type)) * weightSaturation +
                           InvertDiff(luma, GetTargetLuma(type)) * weightLuma;

        const float totalWeight = weightSaturation + weightLuma;

        return totalValue / totalWeight;
    }

    /// <inheritdoc />
    public SKColor[] Quantize(IEnumerable<SKColor> colors, int amount)
    {
        if ((amount & (amount - 1)) != 0)
            throw new ArgumentException("Must be power of two", nameof(amount));

        Queue<ColorCube> cubes = new(amount);
        cubes.Enqueue(new ColorCube(colors));

        while (cubes.Count < amount)
        {
            ColorCube cube = cubes.Dequeue();
            if (cube.TrySplit(out ColorCube? a, out ColorCube? b))
            {
                cubes.Enqueue(a);
                cubes.Enqueue(b);
            }
        }

        return cubes.Select(c => c.GetAverageColor()).ToArray();
    }

    /// <inheritdoc />
    public SKColor FindColorVariation(IEnumerable<SKColor> colors, ColorType type, bool ignoreLimits = false)
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

    /// <inheritdoc />
    public ColorSwatch FindAllColorVariations(IEnumerable<SKColor> colors, bool ignoreLimits = false)
    {
        SKColor bestVibrantColor = SKColor.Empty;
        SKColor bestLightVibrantColor = SKColor.Empty;
        SKColor bestDarkVibrantColor = SKColor.Empty;
        SKColor bestMutedColor = SKColor.Empty;
        SKColor bestLightMutedColor = SKColor.Empty;
        SKColor bestDarkMutedColor = SKColor.Empty;
        float bestVibrantScore = 0;
        float bestLightVibrantScore = 0;
        float bestDarkVibrantScore = 0;
        float bestMutedScore = 0;
        float bestLightMutedScore = 0;
        float bestDarkMutedScore = 0;

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
            DarkMuted = bestDarkMutedColor
        };
    }

    #region Constants

    private const float targetDarkLuma = 0.26f;
    private const float maxDarkLuma = 0.45f;
    private const float minLightLuma = 0.55f;
    private const float targetLightLuma = 0.74f;
    private const float minNormalLuma = 0.3f;
    private const float targetNormalLuma = 0.5f;
    private const float maxNormalLuma = 0.7f;
    private const float targetMutesSaturation = 0.3f;
    private const float maxMutesSaturation = 0.3f;
    private const float targetVibrantSaturation = 1.0f;
    private const float minVibrantSaturation = 0.35f;
    private const float weightSaturation = 3f;
    private const float weightLuma = 5f;

    private static float GetTargetLuma(ColorType colorType)
    {
        return colorType switch
        {
            ColorType.Vibrant => targetNormalLuma,
            ColorType.LightVibrant => targetLightLuma,
            ColorType.DarkVibrant => targetDarkLuma,
            ColorType.Muted => targetNormalLuma,
            ColorType.LightMuted => targetLightLuma,
            ColorType.DarkMuted => targetDarkLuma,
            _ => throw new ArgumentException(nameof(colorType))
        };
    }

    private static float GetMinLuma(ColorType colorType)
    {
        return colorType switch
        {
            ColorType.Vibrant => minNormalLuma,
            ColorType.LightVibrant => minLightLuma,
            ColorType.DarkVibrant => 0f,
            ColorType.Muted => minNormalLuma,
            ColorType.LightMuted => minLightLuma,
            ColorType.DarkMuted => 0,
            _ => throw new ArgumentException(nameof(colorType))
        };
    }

    private static float GetMaxLuma(ColorType colorType)
    {
        return colorType switch
        {
            ColorType.Vibrant => maxNormalLuma,
            ColorType.LightVibrant => 1f,
            ColorType.DarkVibrant => maxDarkLuma,
            ColorType.Muted => maxNormalLuma,
            ColorType.LightMuted => 1f,
            ColorType.DarkMuted => maxDarkLuma,
            _ => throw new ArgumentException(nameof(colorType))
        };
    }

    private static float GetTargetSaturation(ColorType colorType)
    {
        return colorType switch
        {
            ColorType.Vibrant => targetVibrantSaturation,
            ColorType.LightVibrant => targetVibrantSaturation,
            ColorType.DarkVibrant => targetVibrantSaturation,
            ColorType.Muted => targetMutesSaturation,
            ColorType.LightMuted => targetMutesSaturation,
            ColorType.DarkMuted => targetMutesSaturation,
            _ => throw new ArgumentException(nameof(colorType))
        };
    }

    private static float GetMinSaturation(ColorType colorType)
    {
        return colorType switch
        {
            ColorType.Vibrant => minVibrantSaturation,
            ColorType.LightVibrant => minVibrantSaturation,
            ColorType.DarkVibrant => minVibrantSaturation,
            ColorType.Muted => 0,
            ColorType.LightMuted => 0,
            ColorType.DarkMuted => 0,
            _ => throw new ArgumentException(nameof(colorType))
        };
    }

    private static float GetMaxSaturation(ColorType colorType)
    {
        return colorType switch
        {
            ColorType.Vibrant => 1f,
            ColorType.LightVibrant => 1f,
            ColorType.DarkVibrant => 1f,
            ColorType.Muted => maxMutesSaturation,
            ColorType.LightMuted => maxMutesSaturation,
            ColorType.DarkMuted => maxMutesSaturation,
            _ => throw new ArgumentException(nameof(colorType))
        };
    }

    #endregion
}