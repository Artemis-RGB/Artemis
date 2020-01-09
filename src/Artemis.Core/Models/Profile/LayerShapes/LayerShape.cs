using System.Linq;
using Artemis.Storage.Entities.Profile;
using SkiaSharp;

namespace Artemis.Core.Models.Profile.LayerShapes
{
    public abstract class LayerShape
    {
        protected LayerShape(Layer layer)
        {
            Layer = layer;
        }

        protected LayerShape(Layer layer, ShapeEntity shapeEntity)
        {
            Layer = layer;
        }

        /// <summary>
        ///     The layer this shape is attached to
        /// </summary>
        public Layer Layer { get; set; }

        /// <summary>
        ///     A render rectangle relative to the layer
        /// </summary>
        public SKRect RenderRectangle { get; protected set; }

        /// <summary>
        ///     A path relative to the layer
        /// </summary>
        public SKPath RenderPath { get; protected set; }

        public abstract void CalculateRenderProperties(SKPoint shapePosition, SKSize shapeSize);

        public virtual void ApplyToEntity()
        {
            Layer.LayerEntity.ShapeEntity = new ShapeEntity();
        }

        /// <summary>
        ///     Updates Position and Size using the provided unscaled rectangle
        /// </summary>
        /// <param name="rect">An unscaled rectangle where 1px = 1mm</param>
        public void SetFromUnscaledRectangle(SKRect rect)
        {
            if (!Layer.Leds.Any())
            {
                Layer.PositionProperty.Value = SKPoint.Empty;
                Layer.SizeProperty.Value = SKSize.Empty;
                return;
            }

            var x = Layer.Leds.Min(l => l.RgbLed.AbsoluteLedRectangle.Location.X);
            var y = Layer.Leds.Min(l => l.RgbLed.AbsoluteLedRectangle.Location.Y);
            var width = Layer.Leds.Max(l => l.RgbLed.AbsoluteLedRectangle.Location.X + l.RgbLed.AbsoluteLedRectangle.Size.Width) - x;
            var height = Layer.Leds.Max(l => l.RgbLed.AbsoluteLedRectangle.Location.Y + l.RgbLed.AbsoluteLedRectangle.Size.Height) - y;

            Layer.PositionProperty.Value = new SKPoint((float) (100f / width * (rect.Left - x)) / 100f, (float) (100f / height * (rect.Top - y)) / 100f);
            Layer.SizeProperty.Value = new SKSize((float) (100f / width * rect.Width) / 100f, (float) (100f / height * rect.Height) / 100f);

            // TODO: Update keyframes
            CalculateRenderProperties(Layer.PositionProperty.CurrentValue, Layer.SizeProperty.CurrentValue);
        }

        public SKRect GetUnscaledRectangle()
        {
            if (!Layer.Leds.Any())
                return SKRect.Empty;

            var x = Layer.Leds.Min(l => l.RgbLed.AbsoluteLedRectangle.Location.X);
            var y = Layer.Leds.Min(l => l.RgbLed.AbsoluteLedRectangle.Location.Y);
            var width = Layer.Leds.Max(l => l.RgbLed.AbsoluteLedRectangle.Location.X + l.RgbLed.AbsoluteLedRectangle.Size.Width) - x;
            var height = Layer.Leds.Max(l => l.RgbLed.AbsoluteLedRectangle.Location.Y + l.RgbLed.AbsoluteLedRectangle.Size.Height) - y;

            return SKRect.Create(
                (float) (x + width * Layer.PositionProperty.CurrentValue.X),
                (float) (y + height * Layer.PositionProperty.CurrentValue.Y),
                (float) (width * Layer.SizeProperty.CurrentValue.Width),
                (float) (height * Layer.SizeProperty.CurrentValue.Height)
            );
        }
    }
}