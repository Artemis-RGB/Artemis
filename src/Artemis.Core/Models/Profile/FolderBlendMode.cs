using SkiaSharp;

namespace Artemis.Core;

/// <summary>
///     Represents how a <see cref="Folder" /> blends its children into what is below it.
/// </summary>
/// <remarks>
///     Values are persisted and every value but <see cref="PassThrough" /> is the matching <see cref="SKBlendMode" /> plus
///     one; append only, do not reorder.
/// </remarks>
public enum FolderBlendMode
{
    /// <summary>
    ///     The children blend straight into what is below the folder.
    /// </summary>
    PassThrough = 0,

#pragma warning disable CS1591
    Clear = SKBlendMode.Clear + 1,
    Src = SKBlendMode.Src + 1,
    Dst = SKBlendMode.Dst + 1,
    SrcOver = SKBlendMode.SrcOver + 1,
    DstOver = SKBlendMode.DstOver + 1,
    SrcIn = SKBlendMode.SrcIn + 1,
    DstIn = SKBlendMode.DstIn + 1,
    SrcOut = SKBlendMode.SrcOut + 1,
    DstOut = SKBlendMode.DstOut + 1,
    SrcATop = SKBlendMode.SrcATop + 1,
    DstATop = SKBlendMode.DstATop + 1,
    Xor = SKBlendMode.Xor + 1,
    Plus = SKBlendMode.Plus + 1,
    Modulate = SKBlendMode.Modulate + 1,
    Screen = SKBlendMode.Screen + 1,
    Overlay = SKBlendMode.Overlay + 1,
    Darken = SKBlendMode.Darken + 1,
    Lighten = SKBlendMode.Lighten + 1,
    ColorDodge = SKBlendMode.ColorDodge + 1,
    ColorBurn = SKBlendMode.ColorBurn + 1,
    HardLight = SKBlendMode.HardLight + 1,
    SoftLight = SKBlendMode.SoftLight + 1,
    Difference = SKBlendMode.Difference + 1,
    Exclusion = SKBlendMode.Exclusion + 1,
    Multiply = SKBlendMode.Multiply + 1,
    Hue = SKBlendMode.Hue + 1,
    Saturation = SKBlendMode.Saturation + 1,
    Color = SKBlendMode.Color + 1,
    Luminosity = SKBlendMode.Luminosity + 1
#pragma warning restore CS1591
}
