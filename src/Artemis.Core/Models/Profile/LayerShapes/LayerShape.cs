using System.Linq;
using Artemis.Storage.Entities.Profile;
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

        protected LayerShape(Layer layer, ShapeEntity shapeEntity)
        {
            Layer = layer;
            ScaledRectangle = SKRect.Create(shapeEntity.X, shapeEntity.Y, shapeEntity.Width, shapeEntity.Height);
        }

        /// <summary>
        ///     The layer this shape is attached to
        /// </summary>
        public Layer Layer { get; set; }

        /// <summary>
        ///     A relative and scaled rectangle that defines where the shape is located in relation to the layer's size
        ///     <para>Note: scaled means a range of 0.0 to 1.0. 1.0 being full width/height, 0.5 being half</para>
        /// </summary>
        public SKRect ScaledRectangle { get; set; }

        /// <summary>
        ///     A path outlining the shape
        /// </summary>
        public SKPath Path
        {
            get => _path;
            protected set
            {
                _path = value;
                Bounds = value?.Bounds ?? SKRect.Empty;
            }
        }

        /// <summary>
        ///     The bounds of this shape
        /// </summary>
        public SKRect Bounds { get; private set; }

        public abstract void CalculateRenderProperties();

        /// <summary>
        ///     Updates Position and Size using the provided unscaled rectangle
        /// </summary>
        /// <param name="rect">An unscaled rectangle where 1px = 1mm</param>
        public void SetFromUnscaledRectangle(SKRect rect)
        {
            if (!Layer.Leds.Any())
            {
                ScaledRectangle = SKRect.Empty;
                return;
            }

            ScaledRectangle = SKRect.Create(
                100f / Layer.Bounds.Width * rect.Left / 100f,
                100f / Layer.Bounds.Height * rect.Top / 100f,
                100f / Layer.Bounds.Width * rect.Width / 100f,
                100f / Layer.Bounds.Height * rect.Height / 100f
            );
        }

        public SKRect GetUnscaledRectangle()
        {
            if (!Layer.Leds.Any())
                return SKRect.Empty;

            return SKRect.Create(
                Layer.Bounds.Width * ScaledRectangle.Left,
                Layer.Bounds.Height * ScaledRectangle.Top,
                Layer.Bounds.Width * ScaledRectangle.Width,
                Layer.Bounds.Height * ScaledRectangle.Height
            );
        }

        internal virtual void ApplyToEntity()
        {
            Layer.LayerEntity.ShapeEntity = new ShapeEntity
            {
                X = ScaledRectangle.Left,
                Y = ScaledRectangle.Top,
                Width = ScaledRectangle.Width,
                Height = ScaledRectangle.Height
            };
        }
    }
}