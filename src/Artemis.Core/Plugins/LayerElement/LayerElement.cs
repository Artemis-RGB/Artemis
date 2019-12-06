using System;
using Artemis.Core.Models.Profile;
using SkiaSharp;

namespace Artemis.Core.Plugins.LayerElement
{
    public abstract class LayerElement : IDisposable
    {
        protected LayerElement(Layer layer, Guid guid, LayerElementSettings settings, LayerElementDescriptor descriptor)
        {
            Layer = layer;
            Guid = guid;
            Settings = settings;
            Descriptor = descriptor;
        }

        public Layer Layer { get; }
        public Guid Guid { get; }
        public LayerElementSettings Settings { get; }
        public LayerElementDescriptor Descriptor { get; }

        public virtual void Dispose()
        {
        }

        /// <summary>
        ///     Called by the profile editor to populate the layer element properties panel
        /// </summary>
        /// <returns></returns>
        public abstract LayerElementViewModel GetViewModel();

        /// <summary>
        ///     Called before rendering every frame, write your update logic here
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual void Update(double deltaTime)
        {
        }

        /// <summary>
        ///     Allows you to perform rendering on the surface <see cref="SKCanvas" /> before any layer-clipping is applied
        ///     <para>Called before rendering, in the order configured on the layer</para>
        /// </summary>
        /// <param name="framePath"></param>
        /// <param name="canvas">The entire surface canvas</param>
        public virtual void RenderPreProcess(SKPath framePath, SKCanvas canvas)
        {
        }

        /// <summary>
        ///     The main method of rendering anything to the layer. The provided <see cref="SKCanvas" /> is specific to the layer
        ///     and matches it's width and height.
        ///     <para>Called during rendering, in the order configured on the layer</para>
        /// </summary>
        /// <param name="framePath"></param>
        /// <param name="canvas">The layer canvas</param>
        public virtual void Render(SKPath framePath, SKCanvas canvas)
        {
        }

        /// <summary>
        ///     Allows you to modify the <see cref="SKShader" /> used to draw the layer's <see cref="SKBitmap" /> on the
        ///     <see cref="SKCanvas" />.
        ///     <para>Called after rendering, in the order configured on the layer.</para>
        /// </summary>
        /// <param name="framePath"></param>
        /// <param name="canvas"></param>
        /// <param name="bitmap">The bitmap created from the layer canvas</param>
        /// <param name="shader">The current shader used to draw the bitmap on the surface canvas</param>
        /// <returns>The resulting shader used to draw the bitmap on the surface canvas</returns>
        public virtual void RenderPostProcess(SKPath framePath, SKCanvas canvas)
        {
        }
    }
}