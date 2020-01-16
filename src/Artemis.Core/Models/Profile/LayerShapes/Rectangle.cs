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
            RenderRectangle = GetUnscaledRectangle();

            var path = new SKPath();
            path.AddRect(RenderRectangle);
            RenderPath = path;
        }

        public override void ApplyToEntity()
        {
            base.ApplyToEntity();
            Layer.LayerEntity.ShapeEntity.Type = ShapeEntityType.Rectangle;
        }
    }
}