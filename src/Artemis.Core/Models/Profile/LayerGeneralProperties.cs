using SkiaSharp;

namespace Artemis.Core
{
    public class LayerGeneralProperties : LayerPropertyGroup
    {
        [PropertyDescription(Name = "Shape type", Description = "The type of shape to draw in this layer")]
        public EnumLayerProperty<LayerShapeType> ShapeType { get; set; }

        [PropertyDescription(Name = "Resize mode", Description = "How to make the shape adjust to scale changes")]
        public EnumLayerProperty<LayerResizeMode> ResizeMode { get; set; }

        [PropertyDescription(Name = "Blend mode", Description = "How to blend this layer into the resulting image")]
        public EnumLayerProperty<SKBlendMode> BlendMode { get; set; }

        [PropertyDescription(Name = "Brush type", Description = "The type of brush to use for this layer")]
        public LayerBrushReferenceLayerProperty BrushReference { get; set; }

        protected override void PopulateDefaults()
        {
            ShapeType.DefaultValue = LayerShapeType.Rectangle;
            ResizeMode.DefaultValue = LayerResizeMode.Normal;
            BlendMode.DefaultValue = SKBlendMode.SrcOver;
        }

        protected override void EnableProperties()
        {
        }

        protected override void DisableProperties()
        {
        }
    }
}