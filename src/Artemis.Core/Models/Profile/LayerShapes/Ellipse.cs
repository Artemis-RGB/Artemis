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
            var path = new SKPath();
            path.AddOval(GetUnscaledRectangle());
            Path = path;
        }

        internal override void ApplyToEntity()
        {
            base.ApplyToEntity();
            Layer.LayerEntity.ShapeEntity.Type = ShapeEntityType.Ellipse;
        }
    }
}