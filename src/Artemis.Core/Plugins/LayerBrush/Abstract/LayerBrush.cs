using System;
using Artemis.Core.Models.Profile;
using Artemis.Core.Services.Interfaces;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.Plugins.LayerBrush.Abstract
{
    public abstract class LayerBrush<T> : PropertiesLayerBrush<T> where T : LayerPropertyGroup
    {
        protected LayerBrush(Layer layer, LayerBrushDescriptor descriptor) : base(layer, descriptor)
        {
            BrushType = LayerBrushType.Regular;
        }

        /// <summary>
        ///     The main method of rendering anything to the layer. The provided <see cref="SKCanvas" /> is specific to the layer
        ///     and matches it's width and height.
        ///     <para>Called during rendering or layer preview, in the order configured on the layer</para>
        /// </summary>
        /// <param name="canvas">The layer canvas</param>
        /// <param name="canvasInfo"></param>
        /// <param name="path">The path to be filled, represents the shape</param>
        /// <param name="paint">The paint to be used to fill the shape</param>
        public abstract void Render(SKCanvas canvas, SKImageInfo canvasInfo, SKPath path, SKPaint paint);
        
        internal override void InternalRender(SKCanvas canvas, SKImageInfo canvasInfo, SKPath path, SKPaint paint)
        {
            // Move the canvas to the top-left of the render path
            canvas.Translate(path.Bounds.Left, path.Bounds.Top);
            // Pass the render path to the layer brush positioned at 0,0
            path.Transform(SKMatrix.MakeTranslation(path.Bounds.Left * -1, path.Bounds.Top * -1));

            Render(canvas, canvasInfo, path, paint);
        }

        internal override IBrush InternalGetBrush()
        {
            throw new NotImplementedException("Regular layer brushes do not implement InternalGetBrush");
        }

        internal override void Initialize(ILayerService layerService)
        {
            InitializeProperties(layerService);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public sealed override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}