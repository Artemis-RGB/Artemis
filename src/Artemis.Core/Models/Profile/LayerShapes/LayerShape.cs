using SkiaSharp;

namespace Artemis.Core.Models.Profile.LayerShapes
{
    public abstract class LayerShape
    {
        private SKPath _path;

        protected LayerShape(Layer layer)
        {
            Layer = layer;
        }

        /// <summary>
        ///     The layer this shape is attached to
        /// </summary>
        public Layer Layer { get; set; }

        /// <summary>
        ///     Gets a copy of the path outlining the shape
        /// </summary>
        public SKPath Path
        {
            get => _path != null ? new SKPath(_path) : null;
            protected set => _path = value;
        }

        public abstract void CalculateRenderProperties();
    }
}