using System.Collections.Generic;
using Artemis.Core;

namespace Artemis.UI.Shared;

/// <summary>
///     Represents an entry in the <see cref="BlendModePicker" />: a blend mode or the header of a group of blend modes.
/// </summary>
public class BlendModeOption
{
    private BlendModeOption(FolderBlendMode? value, string name, string? description)
    {
        Value = value;
        Name = name;
        Description = description;
    }

    /// <summary>
    ///     Gets every option in the order the picker shows them, with a header before each group.
    /// </summary>
    public static IReadOnlyList<BlendModeOption> All { get; } =
    [
        Mode(FolderBlendMode.PassThrough, "Pass through", "Each layer in the folder blends with what's below the folder on its own"),
        Mode(FolderBlendMode.SrcOver, "Normal", "Draws on top of what's below"),

        Header("Darken"),
        Mode(FolderBlendMode.Darken, "Darken", "Keeps the darker of the two colors"),
        Mode(FolderBlendMode.Multiply, "Multiply", "Darkens what's below, white leaves it unchanged"),
        Mode(FolderBlendMode.ColorBurn, "Color burn", "Darkens what's below strongly, white leaves it unchanged"),

        Header("Lighten"),
        Mode(FolderBlendMode.Lighten, "Lighten", "Keeps the lighter of the two colors"),
        Mode(FolderBlendMode.Screen, "Screen", "Lightens what's below, black leaves it unchanged"),
        Mode(FolderBlendMode.ColorDodge, "Color dodge", "Lightens what's below strongly, black leaves it unchanged"),
        Mode(FolderBlendMode.Plus, "Plus", "Adds the colors together, red and blue make magenta"),

        Header("Contrast"),
        Mode(FolderBlendMode.Overlay, "Overlay", "Makes the dark parts of what's below darker and the light parts lighter"),
        Mode(FolderBlendMode.SoftLight, "Soft light", "A gentler overlay"),
        Mode(FolderBlendMode.HardLight, "Hard light", "An overlay driven by this element's colors instead of what's below"),

        Header("Compare"),
        Mode(FolderBlendMode.Difference, "Difference", "Subtracts the darker color from the lighter one"),
        Mode(FolderBlendMode.Exclusion, "Exclusion", "A softer difference"),

        Header("Color"),
        Mode(FolderBlendMode.Hue, "Hue", "Takes the hue of this element and the rest from what's below"),
        Mode(FolderBlendMode.Saturation, "Saturation", "Takes the saturation of this element and the rest from what's below"),
        Mode(FolderBlendMode.Color, "Color", "Takes the hue and saturation of this element and the brightness of what's below"),
        Mode(FolderBlendMode.Luminosity, "Luminosity", "Takes the brightness of this element and the color of what's below"),

        Header("Masking"),
        Mode(FolderBlendMode.SrcIn, "Src in", "Shows this element only where something is below, replacing it"),
        Mode(FolderBlendMode.DstIn, "Dst in", "Keeps what's below only where this element is"),
        Mode(FolderBlendMode.SrcOut, "Src out", "Shows this element only where nothing is below"),
        Mode(FolderBlendMode.DstOut, "Dst out", "Cuts this element's shape out of what's below"),
        Mode(FolderBlendMode.SrcATop, "Src atop", "Draws on top, but only where something is below"),
        Mode(FolderBlendMode.DstATop, "Dst atop", "Keeps what's below only where this element is, with this element behind it"),
        Mode(FolderBlendMode.Xor, "Xor", "Shows both, except where they overlap"),
        Mode(FolderBlendMode.Modulate, "Modulate", "Like multiply, but transparent parts of this element also erase what's below"),

        Header("Other"),
        Mode(FolderBlendMode.Clear, "Clear", "Erases what's below where this element is"),
        Mode(FolderBlendMode.Src, "Src", "Replaces what's below, transparent parts included"),
        Mode(FolderBlendMode.Dst, "Dst", "Hides this element and keeps what's below"),
        Mode(FolderBlendMode.DstOver, "Dst over", "Draws behind what's below")
    ];

    /// <summary>
    ///     Gets the blend mode of the option, or <see langword="null" /> for a group header.
    /// </summary>
    public FolderBlendMode? Value { get; }

    /// <summary>
    ///     Gets the name of the blend mode or group.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets a short explanation of what the blend mode does.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    ///     Gets a value indicating whether the option is the header of a group.
    /// </summary>
    public bool IsHeader => Value == null;

    private static BlendModeOption Mode(FolderBlendMode value, string name, string description)
    {
        return new BlendModeOption(value, name, description);
    }

    private static BlendModeOption Header(string name)
    {
        return new BlendModeOption(null, name, null);
    }
}
