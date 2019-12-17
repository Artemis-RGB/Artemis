using System;
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
        public SKPoint Position { get; set; }

        /// <summary>
        ///     The size of the shape
        /// </summary>
        public SKSize Size { get; set; }

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
                Anchor = new ShapePointEntity { X = Anchor.X, Y = Anchor.Y },
                Position = new ShapePointEntity { X = Position.X, Y = Position.Y },
                Width = Size.Width,
                Height = Size.Height
            };
        }
    }
}