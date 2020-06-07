using System;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Services.Interfaces;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core.Plugins.LayerEffect
{
    /// <summary>
    ///     For internal use only, please use <see cref="LayerEffect" /> instead
    /// </summary>
    public abstract class LayerEffect<T> : BaseLayerEffect where T : LayerPropertyGroup
    {
        private T _properties;

        protected LayerEffect(Layer layer, LayerEffectDescriptor descriptor) : base(layer, descriptor)
        {
        }

        /// <summary>
        ///     Gets whether all properties on this brush are initialized
        /// </summary>
        public bool PropertiesInitialized { get; internal set; }

        /// <inheritdoc />
        public override LayerPropertyGroup BaseProperties => Properties;

        /// <summary>
        ///     Gets the properties of this brush.
        /// </summary>
        public T Properties
        {
            get
            {
                // I imagine a null reference here can be confusing, so lets throw an exception explaining what to do
                if (_properties == null)
                    throw new ArtemisPluginException("Cannot access brush properties until OnPropertiesInitialized has been called");
                return _properties;
            }
            internal set => _properties = value;
        }

        /// <summary>
        ///     Called when all layer properties in this brush have been initialized
        /// </summary>
        protected virtual void OnPropertiesInitialized()
        {
        }

        internal void InitializeProperties(ILayerService layerService)
        {
            Properties = Activator.CreateInstance<T>();
            Properties.InitializeProperties(layerService, Layer, "LayerEffect.");
            OnPropertiesInitialized();
            PropertiesInitialized = true;
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