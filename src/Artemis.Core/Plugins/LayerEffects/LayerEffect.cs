using System;

namespace Artemis.Core.LayerEffects
{
    /// <summary>
    ///     Represents an effect that applies preprocessing and/or postprocessing to a layer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class LayerEffect<T> : BaseLayerEffect where T : LayerPropertyGroup
    {
        private T _properties;

        /// <summary>
        ///     Gets whether all properties on this effect are initialized
        /// </summary>
        public bool PropertiesInitialized { get; internal set; }

        /// <inheritdoc />
        public override LayerPropertyGroup BaseProperties => Properties;

        /// <summary>
        ///     Gets the properties of this effect.
        /// </summary>
        public T Properties
        {
            get
            {
                // I imagine a null reference here can be confusing, so lets throw an exception explaining what to do
                if (_properties == null)
                    throw new ArtemisPluginException("Cannot access effect properties until OnPropertiesInitialized has been called");
                return _properties;
            }
            internal set => _properties = value;
        }

        internal void InitializeProperties()
        {
            Properties = Activator.CreateInstance<T>();
            Properties.LayerEffect = this;
            Properties.Initialize(ProfileElement, PropertyRootPath, Descriptor.Provider);
            PropertiesInitialized = true;

            EnableLayerEffect();
        }

        internal override void Initialize()
        {
            InitializeProperties();
        }
    }
}