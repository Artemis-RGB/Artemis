using System;
using System.Collections.Generic;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Services.Interfaces;

namespace Artemis.Core.Plugins.LayerBrush
{
    public abstract class LayerBrush<T> : BaseLayerBrush where T : LayerPropertyGroup
    {
        private T _properties;

        protected LayerBrush(Layer layer, LayerBrushDescriptor descriptor)
        {
            Layer = layer;
            Descriptor = descriptor;
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

        /// <inheritdoc/>
        public override LayerPropertyGroup BaseProperties => Properties;

        internal override void InitializeProperties(ILayerService layerService, string path)
        {
            Properties.InitializeProperties(layerService, Layer, path);
            OnPropertiesInitialized();
            PropertiesInitialized = true;
        }

        internal virtual void ApplyToEntity()
        {
            Properties.ApplyToEntity();
        }

        internal virtual void OverrideProperties(TimeSpan overrideTime)
        {
            Properties.Override(overrideTime);
        }

        internal virtual IReadOnlyCollection<BaseLayerProperty> GetAllLayerProperties()
        {
            return Properties.GetAllLayerProperties();
        }

        #endregion
    }
}