using SkiaSharp;

namespace Artemis.Core.Models.Profile.LayerShapes
{
    public class Rectangle : LayerShape
    {
        public Rectangle(Layer layer) : base(layer)
        {
        }

        public override void CalculateRenderProperties()
        {
            var path = new SKPath();
            path.AddRect(SKRect.Create(Layer.Bounds.Width, Layer.Bounds.Height));
            Path = path;
        }
    }
}