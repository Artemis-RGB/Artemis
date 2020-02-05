using Artemis.Storage.Entities.Profile;
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
            path.AddRect(Layer.Bounds);
            Path = path;
        }
    }
}