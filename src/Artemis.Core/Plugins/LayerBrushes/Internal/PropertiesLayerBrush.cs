using System;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core.LayerBrushes
{
    /// <summary>
    ///     For internal use only, please use <see cref="LayerBrush{T}" /> or <see cref="PerLedLayerBrush{T}" /> or instead
    /// </summary>
    public abstract class PropertiesLayerBrush<T> : BaseLayerBrush where T : LayerPropertyGroup, new()
    {
        private T _properties = null!;

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
                    throw new InvalidOperationException("Cannot access brush properties until OnPropertiesInitialized has been called");
                return _properties;
            }
            internal set => _properties = value;
        }

        internal void InitializeProperties(PropertyGroupEntity? propertyGroupEntity)
        {
            Properties = new T();
            PropertyGroupDescriptionAttribute groupDescription = new() {Identifier = "Brush", Name = Descriptor.DisplayName, Description = Descriptor.Description};
            Properties.Initialize(Layer, null, groupDescription, propertyGroupEntity);
            PropertiesInitialized = true;

            EnableLayerBrush();
        }
    }
}