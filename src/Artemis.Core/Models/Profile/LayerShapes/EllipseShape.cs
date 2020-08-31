using SkiaSharp;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents an ellipse layer shape
    /// </summary>
    public class EllipseShape : LayerShape
    {
        internal EllipseShape(Layer layer) : base(layer)
        {
        }

        /// <inheritdoc />
        public override void CalculateRenderProperties()
        {
            var path = new SKPath();
            path.AddOval(SKRect.Create(Layer.Bounds.Width, Layer.Bounds.Height));
            Path = path;
        }
    }
}