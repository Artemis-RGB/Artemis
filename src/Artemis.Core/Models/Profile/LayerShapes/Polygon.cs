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
        public List<SKPoint> RenderPoints
        {
            get
            {
                var x = Layer.Leds.Min(l => l.RgbLed.AbsoluteLedRectangle.Location.X);
                var y = Layer.Leds.Min(l => l.RgbLed.AbsoluteLedRectangle.Location.Y);
                var width = Layer.Leds.Max(l => l.RgbLed.AbsoluteLedRectangle.Location.X + l.RgbLed.AbsoluteLedRectangle.Size.Width) - x;
                var height = Layer.Leds.Max(l => l.RgbLed.AbsoluteLedRectangle.Location.Y + l.RgbLed.AbsoluteLedRectangle.Size.Height) - y;
                return Points.Select(p => new SKPoint((float) (p.X * width), (float) (p.Y * height))).ToList();
            }
        }

        public override void CalculateRenderProperties()
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