using System;
using System.Linq;
using Artemis.Storage.Entities.Profile;
using SkiaSharp;

namespace Artemis.Core.Models.Profile.LayerShapes
{
    public abstract class LayerShape
    {
        private SKPoint _position;
        private SKSize _size;

        protected LayerShape(Layer layer)
        {
            Layer = layer;
        }

        protected LayerShape(Layer layer, ShapeEntity shapeEntity)
        {
            Layer = layer;
            Anchor = new SKPoint(shapeEntity.Anchor?.X ?? 0, shapeEntity.Anchor?.Y ?? 0);
            Position = new SKPoint(shapeEntity.Position?.X ?? 0, shapeEntity.Position?.Y ?? 0);
            Size = new SKSize(shapeEntity.Width, shapeEntity.Height);
        }


        /// <summary>
        ///     The layer this shape is attached to
        /// </summary>
        public Layer Layer { get; set; }

        /// <summary>
        ///     At which position the shape is attached to the layer
        /// </summary>
        public SKPoint Anchor { get; set; }

        /// <summary>
        ///     The position of the shape
        /// </summary>
        public SKPoint Position
        {
            get => _position;
            set
            {
                _position = value;
                Layer.CalculateRenderProperties();
            }
        }

        /// <summary>
        ///     The size of the shape
        /// </summary>
        public SKSize Size
        {
            get => _size;
            set
            {
                _size = value;
                Layer.CalculateRenderProperties();
            }
        }

        /// <summary>
        /// A render rectangle relative to the layer
        /// </summary>
        public SKRect RenderRectangle { get; protected set; }

        /// <summary>
        /// A path relative to the layer
        /// </summary>
        public SKPath RenderPath { get; protected set; }

        public abstract void CalculateRenderProperties();

        public virtual void ApplyToEntity()
        {
            Layer.LayerEntity.ShapeEntity = new ShapeEntity
            {
                Anchor = new ShapePointEntity {X = Anchor.X, Y = Anchor.Y},
                Position = new ShapePointEntity {X = Position.X, Y = Position.Y},
                Width = Size.Width,
                Height = Size.Height
            };
        }

        /// <summary>
        /// Updates Position and Size using the provided unscaled rectangle
        /// </summary>
        /// <param name="rect">An unscaled rectangle where 1px = 1mm</param>
        public void SetFromUnscaledRectangle(SKRect rect)
        {
            if (!Layer.Leds.Any())
            {
                Position = SKPoint.Empty;
                Size = SKSize.Empty;
                return;
            }

            var x = Layer.Leds.Min(l => l.RgbLed.AbsoluteLedRectangle.Location.X);
            var y = Layer.Leds.Min(l => l.RgbLed.AbsoluteLedRectangle.Location.Y);
            var width = Layer.Leds.Max(l => l.RgbLed.AbsoluteLedRectangle.Location.X + l.RgbLed.AbsoluteLedRectangle.Size.Width) - x;
            var height = Layer.Leds.Max(l => l.RgbLed.AbsoluteLedRectangle.Location.Y + l.RgbLed.AbsoluteLedRectangle.Size.Height) - y;

            Position = new SKPoint((float) (100f / width * (rect.Left - x)) / 100f, (float) (100f / height * (rect.Top - y)) / 100f);
            Size = new SKSize((float) (100f / width * rect.Width) / 100f, (float) (100f / height * rect.Height) / 100f);
        }

        public SKRect GetUnscaledRectangle()
        {
            var x = Layer.Leds.Min(l => l.RgbLed.AbsoluteLedRectangle.Location.X);
            var y = Layer.Leds.Min(l => l.RgbLed.AbsoluteLedRectangle.Location.Y);
            var width = Layer.Leds.Max(l => l.RgbLed.AbsoluteLedRectangle.Location.X + l.RgbLed.AbsoluteLedRectangle.Size.Width) - x;
            var height = Layer.Leds.Max(l => l.RgbLed.AbsoluteLedRectangle.Location.Y + l.RgbLed.AbsoluteLedRectangle.Size.Height) - y;

            return SKRect.Create(
                (float) (x + width * Position.X),
                (float) (y + height * Position.Y),
                (float) (width * Size.Width),
                (float) (height * Size.Height)
            );
        }
    }
}