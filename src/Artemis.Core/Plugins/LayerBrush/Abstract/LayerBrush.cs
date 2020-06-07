using Artemis.Core.Models.Profile;
using Artemis.Core.Services.Interfaces;
using SkiaSharp;

namespace Artemis.Core.Plugins.LayerBrush.Abstract
{
    public abstract class LayerBrush<T> : PropertiesLayerBrush<T> where T : LayerPropertyGroup
    {
        protected LayerBrush()
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

        internal override void Initialize(ILayerService layerService)
        {
            InitializeProperties(layerService);
        }
    }
}