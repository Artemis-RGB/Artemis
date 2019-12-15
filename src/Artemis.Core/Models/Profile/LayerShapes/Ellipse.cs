using Artemis.Storage.Entities.Profile;
using SkiaSharp;

namespace Artemis.Core.Models.Profile.LayerShapes
{
    public class Ellipse : LayerShape
    {
        public Ellipse(Layer layer) : base(layer)
        {
        }

        internal Ellipse(Layer layer, ShapeEntity shapeEntity) : base(layer, shapeEntity)
        {
        }

        public override void CalculateRenderProperties()
        {
            var width = Layer.AbsoluteRectangle.Width;
            var height = Layer.AbsoluteRectangle.Height;
            var rect = SKRect.Create(Position.X * width, Position.Y * height, Size.Width * width, Size.Height * height);

            var path = new SKPath();
            path.AddOval(rect);

            RenderPath = path;
            RenderRectangle = path.GetRect();
        }

        public override void ApplyToEntity()
        {
            base.ApplyToEntity();
            Layer.LayerEntity.ShapeEntity.Type = ShapeEntityType.Ellipse;
        }
    }
}