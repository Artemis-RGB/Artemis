using System;
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
        /// <param name="rect">An unscaled rectangle which is relative to the layer (1.0 being full width/height, 0.5 being half).</param>
        /// <param name="time">
        ///     An optional timespan to indicate where to set the properties, if null the properties' base values
        ///     will be used.
        /// </param>
        public void SetFromUnscaledRectangle(SKRect rect, TimeSpan? time)
        {
            if (!Layer.Leds.Any())
            {
                Layer.PositionProperty.SetCurrentValue(SKPoint.Empty, time);
                Layer.SizeProperty.SetCurrentValue(SKSize.Empty, time);
                return;
            }

            var x = Layer.Leds.Min(l => l.RgbLed.AbsoluteLedRectangle.Location.X);
            var y = Layer.Leds.Min(l => l.RgbLed.AbsoluteLedRectangle.Location.Y);
            var width = Layer.Leds.Max(l => l.RgbLed.AbsoluteLedRectangle.Location.X + l.RgbLed.AbsoluteLedRectangle.Size.Width) - x;
            var height = Layer.Leds.Max(l => l.RgbLed.AbsoluteLedRectangle.Location.Y + l.RgbLed.AbsoluteLedRectangle.Size.Height) - y;

            Layer.PositionProperty.SetCurrentValue(new SKPoint((float) (100f / width * (rect.Left - x)) / 100f, (float) (100f / height * (rect.Top - y)) / 100f), time);
            Layer.SizeProperty.SetCurrentValue(new SKSize((float) (100f / width * rect.Width) / 100f, (float) (100f / height * rect.Height) / 100f), time);

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

        public void SetFromUnscaledAnchor(SKPoint anchor, TimeSpan? time)
        {
            if (!Layer.Leds.Any())
            {
                Layer.PositionProperty.SetCurrentValue(SKPoint.Empty, time);
                Layer.SizeProperty.SetCurrentValue(SKSize.Empty, time);
                return;
            }

            var x = Layer.Leds.Min(l => l.RgbLed.AbsoluteLedRectangle.Location.X);
            var y = Layer.Leds.Min(l => l.RgbLed.AbsoluteLedRectangle.Location.Y);
            var width = Layer.Leds.Max(l => l.RgbLed.AbsoluteLedRectangle.Location.X + l.RgbLed.AbsoluteLedRectangle.Size.Width) - x;
            var height = Layer.Leds.Max(l => l.RgbLed.AbsoluteLedRectangle.Location.Y + l.RgbLed.AbsoluteLedRectangle.Size.Height) - y;

            Layer.AnchorPointProperty.SetCurrentValue(new SKPoint(
                (float) (100f / width * (anchor.X - x - Layer.PositionProperty.CurrentValue.X)) / 100f,
                (float) (100f / height * (anchor.Y - y - Layer.PositionProperty.CurrentValue.Y)) / 100f
            ), time);
            CalculateRenderProperties(Layer.PositionProperty.CurrentValue, Layer.SizeProperty.CurrentValue);
        }

        public SKPoint GetUnscaledAnchor()
        {
            if (!Layer.Leds.Any())
                return SKPoint.Empty;

            var x = Layer.Leds.Min(l => l.RgbLed.AbsoluteLedRectangle.Location.X);
            var y = Layer.Leds.Min(l => l.RgbLed.AbsoluteLedRectangle.Location.Y);
            var width = Layer.Leds.Max(l => l.RgbLed.AbsoluteLedRectangle.Location.X + l.RgbLed.AbsoluteLedRectangle.Size.Width) - x;
            var height = Layer.Leds.Max(l => l.RgbLed.AbsoluteLedRectangle.Location.Y + l.RgbLed.AbsoluteLedRectangle.Size.Height) - y;

            return new SKPoint(
                (float) (x + width * (Layer.AnchorPointProperty.CurrentValue.X + Layer.PositionProperty.CurrentValue.X)),
                (float) (y + height * (Layer.AnchorPointProperty.CurrentValue.Y + Layer.PositionProperty.CurrentValue.Y))
            );
        }
    }
}