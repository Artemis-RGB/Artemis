using SkiaSharp;

#pragma warning disable 8618

namespace Artemis.Core;

/// <summary>
///     Represents the general properties of a layer
/// </summary>
public class LayerGeneralProperties : LayerPropertyGroup
{
    /// <summary>
    ///     The type of brush to use for this layer
    /// </summary>
    [PropertyDescription(Name = "Brush type", Description = "The type of brush to use for this layer")]
    public LayerBrushReferenceLayerProperty BrushReference { get; set; }

    /// <summary>
    ///     The type of shape to draw in this layer
    /// </summary>
    [PropertyDescription(Name = "Shape type", Description = "The type of shape to draw in this layer")]
    public EnumLayerProperty<LayerShapeType> ShapeType { get; set; }

    /// <summary>
    ///     How to blend this layer into the resulting image
    /// </summary>
    [PropertyDescription(Name = "Blend mode", Description = "How to blend this layer into the resulting image")]
    public EnumLayerProperty<SKBlendMode> BlendMode { get; set; }

    /// <summary>
    ///     How the transformation properties are applied to the layer
    /// </summary>
    [PropertyDescription(Name = "Transform mode", Description = "How the transformation properties are applied to the layer")]
    public EnumLayerProperty<LayerTransformMode> TransformMode { get; set; }

    /// <inheritdoc />
    protected override void PopulateDefaults()
    {
        ShapeType.DefaultValue = LayerShapeType.Rectangle;
        BlendMode.DefaultValue = SKBlendMode.SrcOver;
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