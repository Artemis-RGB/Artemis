using Artemis.Storage.Entities.Profile;
using SkiaSharp;

namespace Artemis.Core.Models.Profile.LayerShapes
{
    public class Ellipse : LayerShape
    {
        public Ellipse(Layer layer) : base(layer)
        {
        }

        public override void CalculateRenderProperties()
        {
            var path = new SKPath();
            path.AddOval(Layer.Bounds);
            Path = path;
        }

        public override void ApplyToEntity()
        {
            Layer.LayerEntity.ShapeType = ShapeEntityType.Ellipse;
        }
    }
}