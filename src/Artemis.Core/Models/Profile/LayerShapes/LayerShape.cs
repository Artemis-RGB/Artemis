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
        ///     An absolute and scaled render rectangle
        /// </summary>
        public SKRect RenderRectangle { get; protected set; }

        /// <summary>
        ///     A path relative to the layer
        /// </summary>
        public SKPath RenderPath { get; protected set; }

        public abstract void CalculateRenderProperties();

        public virtual void ApplyToEntity()
        {
            Layer.LayerEntity.ShapeEntity = new ShapeEntity
            {
                X = ScaledRectangle.Left,
                Y = ScaledRectangle.Top,
                Width = ScaledRectangle.Width,
                Height = ScaledRectangle.Height
            };
        }

        protected SKRect GetUnscaledRectangle()
        {
            if (!Layer.Leds.Any())
                return SKRect.Empty;

            return SKRect.Create(
                Layer.AbsoluteRectangle.Left + Layer.AbsoluteRectangle.Width * ScaledRectangle.Left,
                Layer.AbsoluteRectangle.Top + Layer.AbsoluteRectangle.Height * ScaledRectangle.Top,
                Layer.AbsoluteRectangle.Width * ScaledRectangle.Width,
                Layer.AbsoluteRectangle.Height * ScaledRectangle.Height
            );
        }
    }
}