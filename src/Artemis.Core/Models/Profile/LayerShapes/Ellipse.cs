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
            path.AddOval(SKRect.Create(Layer.Bounds.Width, Layer.Bounds.Height));
            Path = path;
        }
    }
}