using Artemis.Storage.Entities.Profile;
using SkiaSharp;

namespace Artemis.Core.Models.Profile.LayerShapes
{
    public class Rectangle : LayerShape
    {
        public Rectangle(Layer layer) : base(layer)
        {
        }

        internal Rectangle(Layer layer, ShapeEntity shapeEntity) : base(layer, shapeEntity)
        {
        }

        public override void CalculateRenderProperties()
        {
            var path = new SKPath();
            path.AddRect(GetUnscaledRectangle());
            Path = path;
        }

        internal override void ApplyToEntity()
        {
            base.ApplyToEntity();
            Layer.LayerEntity.ShapeEntity.Type = ShapeEntityType.Rectangle;
        }
    }
}