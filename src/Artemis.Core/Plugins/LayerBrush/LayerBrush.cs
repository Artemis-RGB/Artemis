using System;
using Artemis.Core.Models.Profile;
using SkiaSharp;

namespace Artemis.Core.Plugins.LayerBrush
{
    public abstract class LayerBrush : IDisposable
    {
        protected LayerBrush(Layer layer, LayerBrushSettings settings, LayerBrushDescriptor descriptor)
        {
            Layer = layer;
            Settings = settings;
            Descriptor = descriptor;
        }

        public Layer Layer { get; }
        public LayerBrushSettings Settings { get; }
        public LayerBrushDescriptor Descriptor { get; }

        public virtual void Dispose()
        {
        }

        /// <summary>
        ///     Called by the profile editor to populate the brush properties panel
        /// </summary>
        /// <returns></returns>
        public abstract LayerBrushViewModel GetViewModel();

        /// <summary>
        ///     Called before rendering every frame, write your update logic here
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual void Update(double deltaTime)
        {
        }

        /// <summary>
        ///     The main method of rendering anything to the layer. The provided <see cref="SKCanvas" /> is specific to the layer
        ///     and matches it's width and height.
        ///     <para>Called during rendering, in the order configured on the layer</para>
        /// </summary>
        /// <param name="canvas">The layer canvas</param>
        /// <param name="path">The path to be filled, represents the shape</param>
        /// <param name="paint">The paint to be used to fill the shape</param>
        public virtual void Render(SKCanvas canvas, SKPath path, SKPaint paint)
        {
        }
    }
}