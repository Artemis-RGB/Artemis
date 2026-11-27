using SkiaSharp;

#pragma warning disable 8618

namespace Artemis.Core;

/// <summary>
///     Represents the general properties of a folder.
/// </summary>
public class FolderGeneralProperties : LayerPropertyGroup
{
    /// <summary>
    ///     Gets or sets how the folder blends its children into what is below it.
    /// </summary>
    /// <remarks>
    ///     Not used on the root folder, which follows <see cref="ProfileConfiguration.BlendMode" />.
    /// </remarks>
    [PropertyDescription(Name = "Blend mode", Description = "How to blend this folder into what is below it, pass through lets each child blend on its own")]
    public EnumLayerProperty<FolderBlendMode> BlendMode { get; set; }

    /// <summary>
    ///     Converts a folder blend mode to the matching Skia blend mode, <see cref="FolderBlendMode.PassThrough" /> becomes
    ///     <see cref="SKBlendMode.SrcOver" />.
    /// </summary>
    public static SKBlendMode ToSKBlendMode(FolderBlendMode blendMode)
    {
        return blendMode == FolderBlendMode.PassThrough ? SKBlendMode.SrcOver : (SKBlendMode) (blendMode - 1);
    }

    /// <summary>
    ///     Converts a Skia blend mode to the matching folder blend mode.
    /// </summary>
    public static FolderBlendMode FromSKBlendMode(SKBlendMode blendMode)
    {
        return (FolderBlendMode) (blendMode + 1);
    }

    /// <inheritdoc />
    protected override void PopulateDefaults()
    {
        BlendMode.DefaultValue = FolderBlendMode.SrcOver;
    }

    /// <inheritdoc />
    protected override void EnableProperties()
    {
    }

    /// <inheritdoc />
    protected override void DisableProperties()
    {
    }
}
