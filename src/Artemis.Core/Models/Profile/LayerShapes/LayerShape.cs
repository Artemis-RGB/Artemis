using SkiaSharp;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents the shape of a layer
    /// </summary>
    public abstract class LayerShape
    {
        internal LayerShape(Layer layer)
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
        public SKPath? Path { get; protected set; }

        /// <summary>
        ///     Calculates the <see cref="Path" />
        /// </summary>
        public abstract void CalculateRenderProperties();
    }
}