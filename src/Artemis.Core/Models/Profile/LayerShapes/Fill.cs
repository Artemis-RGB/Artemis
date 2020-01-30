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
            // Shape originates from the center so compensate the path for that
            Path = new SKPath(Layer.Path);
        }

        internal override void ApplyToEntity()
        {
            base.ApplyToEntity();
            Layer.LayerEntity.ShapeEntity.Type = ShapeEntityType.Fill;
        }
    }
}