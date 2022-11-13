using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using Artemis.Core.ColorScience;

namespace Artemis.Core.Services;

/// <inheritdoc />
[Obsolete]
internal class ColorQuantizerService : IColorQuantizerService
{
    /// <inheritdoc />
    public SKColor[] Quantize(IEnumerable<SKColor> colors, int amount)
    {
        return  ColorQuantizer.Quantize(colors.ToArray(), amount);
    }

    /// <inheritdoc />
    public SKColor FindColorVariation(IEnumerable<SKColor> colors, ColorType type, bool ignoreLimits = false)
    {
        return ColorQuantizer.FindColorVariation(colors, (ColorScience.ColorType)type, ignoreLimits);
    }

    /// <inheritdoc />
    public ColorSwatch FindAllColorVariations(IEnumerable<SKColor> colors, bool ignoreLimits = false)
    {
        var differentSwatch = ColorQuantizer.FindAllColorVariations(colors, ignoreLimits);

        return new()
        {
            Vibrant = differentSwatch.Vibrant,
            LightVibrant = differentSwatch.LightVibrant,
            DarkVibrant = differentSwatch.DarkVibrant,
            Muted = differentSwatch.Muted,
            LightMuted = differentSwatch.LightMuted,
            DarkMuted = differentSwatch.DarkMuted
        };
    }
}