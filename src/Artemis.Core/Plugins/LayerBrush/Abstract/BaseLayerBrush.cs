using System;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.Plugins.LayerBrush.Abstract
{
    /// <summary>
    ///     For internal use only, please use <see cref="LayerBrush{T}" /> or <see cref="RgbNetLayerBrush{T}" /> or instead
    /// </summary>
    public abstract class BaseLayerBrush : IDisposable
    {
        protected BaseLayerBrush(Layer layer, LayerBrushDescriptor descriptor)
        {
            Layer = layer;
            Descriptor = descriptor;
        }

        /// <summary>
        ///     Gets the layer this brush is applied to
        /// </summary>
        public Layer Layer { get; internal set; }

        /// <summary>
        ///     Gets the descriptor of this brush
        /// </summary>
        public LayerBrushDescriptor Descriptor { get; internal set; }

        /// <summary>
        ///     Gets the plugin info that defined this brush
        /// </summary>
        public PluginInfo PluginInfo => Descriptor.LayerBrushProvider.PluginInfo;

        /// <summary>
        ///     Gets the type of layer brush
        /// </summary>
        public LayerBrushType BrushType { get; internal set; }

        /// <summary>
        ///     Gets a reference to the layer property group without knowing it's type
        /// </summary>
        public virtual LayerPropertyGroup BaseProperties => null;
        
        /// <summary>
        ///     Called when the brush is being removed from the layer
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        ///     Called before rendering every frame, write your update logic here
        /// </summary>
        /// <param name="deltaTime"></param>
        public abstract void Update(double deltaTime);

        // Not only is this needed to initialize properties on the layer brushes, it also prevents implementing anything
        // but LayerBrush<T> and RgbNetLayerBrush<T> outside the core
        internal abstract void Initialize(ILayerService layerService);

        internal abstract void InternalRender(SKCanvas canvas, SKImageInfo canvasInfo, SKPath path, SKPaint paint);

        /// <summary>
        ///     Called when Artemis needs an instance of the RGB.NET brush you are implementing
        /// </summary>
        /// <returns>Your RGB.NET brush</returns>
        internal abstract IBrush InternalGetBrush();
    }

    public enum LayerBrushType
    {
        Regular,
        RgbNet
    }
}