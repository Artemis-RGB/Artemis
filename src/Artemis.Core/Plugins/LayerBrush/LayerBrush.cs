using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Services.Interfaces;
using SkiaSharp;

namespace Artemis.Core.Plugins.LayerBrush
{
    public abstract class LayerBrush<T> : ILayerBrush where T : LayerPropertyGroup
    {
        private T _properties;

        protected LayerBrush(Layer layer, LayerBrushDescriptor descriptor)
        {
            Layer = layer;
            Descriptor = descriptor;
        }

        /// <inheritdoc />
        public Layer Layer { get; }

        /// <inheritdoc />
        public LayerBrushDescriptor Descriptor { get; }

        /// <inheritdoc />
        public virtual void Dispose()
        {
        }

        /// <inheritdoc />
        public virtual void Update(double deltaTime)
        {
        }

        /// <inheritdoc />
        public virtual void Render(SKCanvas canvas, SKImageInfo canvasInfo, SKPath path, SKPaint paint)
        {
        }

        #region Properties

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
        ///     Gets whether all properties on this brush are initialized
        /// </summary>
        public bool PropertiesInitialized { get; private set; }
        
        /// <summary>
        ///     Called when all layer properties in this brush have been initialized
        /// </summary>
        protected virtual void OnPropertiesInitialized()
        {
        }

        public void InitializeProperties(ILayerService layerService, string path)
        {
            Properties.InitializeProperties(layerService, Descriptor.LayerBrushProvider.PluginInfo, path);
            OnPropertiesInitialized();
            PropertiesInitialized = true;
        }

        #endregion
    }
}