using Artemis.Core.Models.Profile.LayerProperties;
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

        protected override void OnPropertiesInitialized()
        {
            // Populate defaults
            if (!ShapeType.IsLoadedFromStorage)
                ShapeType.BaseValue = LayerShapeType.Rectangle;
            if (!FillType.IsLoadedFromStorage)
                FillType.BaseValue = LayerFillType.Stretch;
            if (!BlendMode.IsLoadedFromStorage)
                BlendMode.BaseValue = SKBlendMode.SrcOver;

            // TODO: SpoinkyNL 28-4-2020: Select preferred default brush type with a fallback to the first available
        }
    }
}