using System.Collections.Generic;
using System.Linq;
using Artemis.Storage.Entities.Profile;
using SkiaSharp;

namespace Artemis.Core.Models.Profile.LayerShapes
{
    public class Polygon : LayerShape
    {
        public Polygon(Layer layer) : base(layer)
        {
        }

        internal Polygon(Layer layer, ShapeEntity shapeEntity) : base(layer, shapeEntity)
        {
        }

        /// <summary>
        ///     The points of this polygon
        /// </summary>
        public List<SKPoint> Points { get; set; }

        /// <summary>
        ///     The points of this polygon where they need to be rendered inside the layer
        /// </summary>
        public List<SKPoint> RenderPoints => Points.Select(p => new SKPoint(p.X * Layer.AbsoluteRectangle.Width, p.Y * Layer.AbsoluteRectangle.Height)).ToList();

        public override void CalculateRenderProperties(SKPoint shapePosition, SKSize shapeSize)
        {
            var path = new SKPath();
            path.AddPoly(RenderPoints.ToArray());

            RenderPath = path;
            RenderRectangle = path.GetRect();
        }

        public override void ApplyToEntity()
        {
            base.ApplyToEntity();
            Layer.LayerEntity.ShapeEntity.Type = ShapeEntityType.Polygon;
            Layer.LayerEntity.ShapeEntity.Points = Points.Select(p => new ShapePointEntity {X = p.X, Y = p.Y}).ToList();
        }
    }
}