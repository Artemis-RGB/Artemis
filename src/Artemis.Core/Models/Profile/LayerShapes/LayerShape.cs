using SkiaSharp;

namespace Artemis.Core.Models.Profile.LayerShapes
{
    public abstract class LayerShape
    {
        protected LayerShape(Layer layer)
        {
            Layer = layer;
        }

        /// <summary>
        ///     The layer this shape is attached to
        /// </summary>
        public Layer Layer { get; set; }

        /// <summary>
        ///     Gets a the path outlining the shape
        /// </summary>
        public SKPath Path { get; protected set; }

        public abstract void CalculateRenderProperties();
    }
}