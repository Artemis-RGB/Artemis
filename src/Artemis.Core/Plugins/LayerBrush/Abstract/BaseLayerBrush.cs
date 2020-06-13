using System;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using SkiaSharp;
using Stylet;

namespace Artemis.Core.Plugins.LayerBrush.Abstract
{
    /// <summary>
    ///     For internal use only, please use <see cref="LayerBrush{T}" /> or <see cref="RgbNetLayerBrush{T}" /> or instead
    /// </summary>
    public abstract class BaseLayerBrush : PropertyChangedBase, IDisposable
    {
        private bool _supportsTransformation = true;

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
        ///     Gets or sets whether the brush supports transformations
        ///     <para>Note: RGB.NET brushes can never be transformed and setting this to true will throw an exception</para>
        /// </summary>
        public bool SupportsTransformation
        {
            get => _supportsTransformation;
            protected set
            {
                if (value && BrushType == LayerBrushType.RgbNet)
                    throw new ArtemisPluginException(PluginInfo, "An RGB.NET brush cannot support transformation");
                _supportsTransformation = value;
            }
        }

        public void Dispose()
        {
            DisableLayerBrush();
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Called when the layer brush is activated
        /// </summary>
        public abstract void EnableLayerBrush();

        /// <summary>
        ///     Called when the layer brush is deactivated
        /// </summary>
        public abstract void DisableLayerBrush();

        /// <summary>
        ///     Called before rendering every frame, write your update logic here
        /// </summary>
        /// <param name="deltaTime"></param>
        public abstract void Update(double deltaTime);

        // Not only is this needed to initialize properties on the layer brushes, it also prevents implementing anything
        // but LayerBrush<T> and RgbNetLayerBrush<T> outside the core
        internal abstract void Initialize(ILayerService layerService);

        internal abstract void InternalRender(SKCanvas canvas, SKImageInfo canvasInfo, SKPath path, SKPaint paint);

        internal virtual void Dispose(bool disposing)
        {
        }
    }

    public enum LayerBrushType
    {
        Regular,
        RgbNet
    }
}