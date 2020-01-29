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

            // Shape originates from the center so compensate the path for that
            var renderPath = new SKPath(Layer.Path);
            renderPath.Transform(SKMatrix.MakeTranslation(RenderRectangle.Left - Layer.Path.Bounds.Left, RenderRectangle.Top - Layer.Path.Bounds.Top));
            RenderPath = renderPath;
        }

        public override void ApplyToEntity()
        {
            base.ApplyToEntity();
            Layer.LayerEntity.ShapeEntity.Type = ShapeEntityType.Fill;
        }
    }
}