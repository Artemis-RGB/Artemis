using Artemis.Storage.Entities.Profile;
using SkiaSharp;

namespace Artemis.Core.Models.Profile.LayerShapes
{
    public class Fill : LayerShape
    {
        public Fill(Layer layer) : base(layer)
        {
        }

        internal Fill(Layer layer, ShapeEntity shapeEntity) : base(layer, shapeEntity)
        {
        }

        public override void CalculateRenderProperties(SKPoint shapePosition, SKSize shapeSize)
        {
            // TODO: Scale the path? Not sure if desirable
            RenderPath = Layer.Path;
            RenderRectangle = Layer.Path.GetRect();
        }

        public override void ApplyToEntity()
        {
            base.ApplyToEntity();
            Layer.LayerEntity.ShapeEntity.Type = ShapeEntityType.Fill;
        }
    }
}