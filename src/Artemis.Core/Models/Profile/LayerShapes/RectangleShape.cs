using SkiaSharp;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a rectangular layer shape
    /// </summary>
    public class RectangleShape : LayerShape
    {
        internal RectangleShape(Layer layer) : base(layer)
        {
        }

        /// <inheritdoc />
        public override void CalculateRenderProperties()
        {
            SKPath path = new SKPath();
            path.AddRect(SKRect.Create(Layer.Bounds.Width, Layer.Bounds.Height));
            Path = path;
        }
    }
}