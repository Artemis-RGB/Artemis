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
            var unscaled = GetUnscaledRectangle();
            RenderRectangle = SKRect.Create(0,0 , unscaled.Width, unscaled.Height);
            
            var path = new SKPath();
            path.AddOval(RenderRectangle);
            RenderPath = path;
        }

        internal override void ApplyToEntity()
        {
            base.ApplyToEntity();
            Layer.LayerEntity.ShapeEntity.Type = ShapeEntityType.Ellipse;
        }
    }
}