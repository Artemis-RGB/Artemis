using System;
using Artemis.Core.Models.Profile;
using Artemis.Core.Services.Interfaces;
using SkiaSharp;

namespace Artemis.Core.Plugins.LayerBrush
{
    public interface ILayerBrush : IDisposable
    {
        /// <summary>
        ///     Gets the layer this brush is applied to
        /// </summary>
        Layer Layer { get; }

        /// <summary>
        ///     Gets the descriptor of this brush
        /// </summary>
        LayerBrushDescriptor Descriptor { get; }

        /// <summary>
        ///     Called before rendering every frame, write your update logic here
        /// </summary>
        /// <param name="deltaTime"></param>
        void Update(double deltaTime);

        /// <summary>
        ///     The main method of rendering anything to the layer. The provided <see cref="SKCanvas" /> is specific to the layer
        ///     and matches it's width and height.
        ///     <para>Called during rendering or layer preview, in the order configured on the layer</para>
        /// </summary>
        /// <param name="canvas">The layer canvas</param>
        /// <param name="canvasInfo"></param>
        /// <param name="path">The path to be filled, represents the shape</param>
        /// <param name="paint">The paint to be used to fill the shape</param>
        void Render(SKCanvas canvas, SKImageInfo canvasInfo, SKPath path, SKPaint paint);

        public void InitializeProperties(ILayerService layerService, string path);
    }
}