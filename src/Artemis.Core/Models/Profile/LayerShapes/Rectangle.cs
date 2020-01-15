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

        public override void CalculateRenderProperties(SKPoint shapePosition, SKSize shapeSize)
        {
            var width = Layer.AbsoluteRectangle.Width;
            var height = Layer.AbsoluteRectangle.Height;
            var rect = SKRect.Create(
                Layer.Rectangle.Left + shapePosition.X * width,
                Layer.Rectangle.Top + shapePosition.Y * height,
                shapeSize.Width * width,
                shapeSize.Height * height
            );
            var path = new SKPath();
            path.AddRect(rect);

            RenderPath = path;
            RenderRectangle = rect;
        }

        public override void ApplyToEntity()
        {
            base.ApplyToEntity();
            Layer.LayerEntity.ShapeEntity.Type = ShapeEntityType.Rectangle;
        }
    }
}