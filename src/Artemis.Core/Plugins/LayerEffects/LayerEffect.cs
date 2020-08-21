using System;
using Artemis.Core.Models.Profile;
using Artemis.Core.Plugins.Exceptions;
using Artemis.Core.Services.Interfaces;

namespace Artemis.Core.Plugins.LayerEffects
{
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

        internal void InitializeProperties(IRenderElementService renderElementService)
        {
            Properties = Activator.CreateInstance<T>();
            Properties.LayerEffect = this;
            Properties.InitializeProperties(renderElementService, ProfileElement, PropertyRootPath);
            PropertiesInitialized = true;

            EnableLayerEffect();
        }

        internal override void Initialize(IRenderElementService renderElementService)
        {
            InitializeProperties(renderElementService);
        }
    }
}