using SkiaSharp;

namespace Artemis.Core.LayerBrushes
{
    /// <summary>
    ///     Represents a brush that renders on a layer
    /// </summary>
    /// <typeparam name="T">The type of brush properties</typeparam>
    public abstract class LayerBrush<T> : PropertiesLayerBrush<T> where T : LayerPropertyGroup
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="LayerBrush{T}" /> class
        /// </summary>
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
        /// <param name="bounds">The area to be filled, covers the shape</param>
        /// <param name="paint">The paint to be used to fill the shape</param>
        public abstract void Render(SKCanvas canvas, SKRect bounds, SKPaint paint);

        internal override void InternalRender(SKCanvas canvas, SKRect bounds, SKPaint paint)
        {
            TryOrBreak(() => Render(canvas, bounds, paint), "Failed to render");
        }

        internal override void Initialize()
        {
            TryOrBreak(() => InitializeProperties(Layer.LayerEntity.LayerBrush?.PropertyGroup), "Failed to initialize");
        }
    }
}