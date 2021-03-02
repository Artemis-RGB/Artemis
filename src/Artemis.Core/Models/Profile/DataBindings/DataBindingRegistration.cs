using System;
using System.Linq;
using Artemis.Storage.Entities.Profile.DataBindings;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class DataBindingRegistration<TLayerProperty, TProperty> : IDataBindingRegistration
    {
        internal DataBindingRegistration(LayerProperty<TLayerProperty> layerProperty, DataBindingConverter<TLayerProperty, TProperty> converter,
            Func<TProperty> getter, Action<TProperty> setter, string displayName)
        {
            LayerProperty = layerProperty ?? throw new ArgumentNullException(nameof(layerProperty));
            Converter = converter ?? throw new ArgumentNullException(nameof(converter));
            Getter = getter ?? throw new ArgumentNullException(nameof(getter));
            Setter = setter ?? throw new ArgumentNullException(nameof(setter));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        }

        /// <summary>
        ///     Gets the layer property this registration was made on
        /// </summary>
        public LayerProperty<TLayerProperty> LayerProperty { get; }

        /// <summary>
        ///     Gets the converter that's used by the data binding
        /// </summary>
        public DataBindingConverter<TLayerProperty, TProperty> Converter { get; }

        /// <summary>
        ///     Gets the function to call to get the value of the property
        /// </summary>
        public Func<TProperty> Getter { get; }

        /// <summary>
        ///     Gets the action to call to set the value of the property
        /// </summary>
        public Action<TProperty> Setter { get; }

        /// <inheritdoc />
        public string DisplayName { get; }

        /// <summary>
        ///     Gets the data binding created using this registration
        /// </summary>
        public DataBinding<TLayerProperty, TProperty>? DataBinding { get; internal set; }

        /// <inheritdoc />
        public IDataBinding? GetDataBinding()
        {
            return DataBinding;
        }

        /// <inheritdoc />
        public IDataBinding? CreateDataBinding()
        {
            if (DataBinding != null)
                return DataBinding;

            DataBindingEntity? dataBinding = LayerProperty.Entity.DataBindingEntities.FirstOrDefault(e => e.Identifier == DisplayName);
            if (dataBinding == null)
                return null;

            DataBinding = new DataBinding<TLayerProperty, TProperty>(LayerProperty, dataBinding);
            return DataBinding;
        }
    }
}