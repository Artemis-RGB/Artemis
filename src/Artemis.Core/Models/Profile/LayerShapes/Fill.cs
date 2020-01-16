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

        public override void CalculateRenderProperties()
        {
            RenderRectangle = GetUnscaledRectangle();
            RenderPath = Layer.Path;
        }

        public override void ApplyToEntity()
        {
            base.ApplyToEntity();
            Layer.LayerEntity.ShapeEntity.Type = ShapeEntityType.Fill;
        }
    }
}