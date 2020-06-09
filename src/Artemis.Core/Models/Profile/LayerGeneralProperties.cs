using Artemis.Core.Models.Profile.LayerProperties.Attributes;
using Artemis.Core.Models.Profile.LayerProperties.Types;
using SkiaSharp;

namespace Artemis.Core.Models.Profile
{
    public class LayerGeneralProperties : LayerPropertyGroup
    {
        [PropertyDescription(Name = "Shape type", Description = "The type of shape to draw in this layer")]
        public EnumLayerProperty<LayerShapeType> ShapeType { get; set; }

        [PropertyDescription(Name = "Fill type", Description = "How to make the shape adjust to scale changes")]
        public EnumLayerProperty<LayerFillType> FillType { get; set; }

        [PropertyDescription(Name = "Blend mode", Description = "How to blend this layer into the resulting image")]
        public EnumLayerProperty<SKBlendMode> BlendMode { get; set; }

        [PropertyDescription(Name = "Brush type", Description = "The type of brush to use for this layer")]
        public LayerBrushReferenceLayerProperty BrushReference { get; set; }

        [PropertyDescription(Name = "Effect type", Description = "The type of effect to use for this layer")]
        public LayerEffectReferenceLayerProperty EffectReference { get; set; }

        protected override void PopulateDefaults()
        {
            ShapeType.DefaultValue = LayerShapeType.Rectangle;
            FillType.DefaultValue = LayerFillType.Stretch;
            BlendMode.DefaultValue = SKBlendMode.SrcOver;
        }

        protected override void OnPropertiesInitialized()
        {
        }
    }
}