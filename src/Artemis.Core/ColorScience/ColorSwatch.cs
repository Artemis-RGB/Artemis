using SkiaSharp;

namespace Artemis.Core.ColorScience;

/// <summary>
///     Swatch containing the known useful color variations.
/// </summary>
public readonly struct ColorSwatch
{
    /// <summary>
    ///     The <see cref="ColorType.Vibrant" /> component.
    /// </summary>
    public readonly SKColor Vibrant { get; init; }

    /// <summary>
    ///     The <see cref="ColorType.LightVibrant" /> component.
    /// </summary>
    public SKColor LightVibrant { get; init; }

    /// <summary>
    ///     The <see cref="ColorType.DarkVibrant" /> component.
    /// </summary>
    public SKColor DarkVibrant { get; init; }

    /// <summary>
    ///     The <see cref="ColorType.Muted" /> component.
    /// </summary>
    public SKColor Muted { get; init; }

    /// <summary>
    ///     The <see cref="ColorType.LightMuted" /> component.
    /// </summary>
    public SKColor LightMuted { get; init; }

    /// <summary>
    ///     The <see cref="ColorType.DarkMuted" /> component.
    /// </summary>
    public SKColor DarkMuted { get; init; }
}