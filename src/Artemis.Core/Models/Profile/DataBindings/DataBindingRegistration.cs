using System;
using System.Linq;
using System.Reflection;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class DataBindingRegistration<TLayerProperty, TProperty> : IDataBindingRegistration
    {
        internal DataBindingRegistration(
            LayerProperty<TLayerProperty> layerProperty,
            DataBindingConverter<TLayerProperty, TProperty> converter,
            PropertyInfo property,
            string path)
        {
            LayerProperty = layerProperty ?? throw new ArgumentNullException(nameof(layerProperty));
            Converter = converter ?? throw new ArgumentNullException(nameof(converter));
            Property = property ?? throw new ArgumentNullException(nameof(property));
            Path = path ?? throw new ArgumentNullException(nameof(path));
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
        ///     Gets the registered property
        /// </summary>
        public PropertyInfo Property { get; }

        /// <summary>
        ///     Gets the path of the registered property on the layer property
        /// </summary>
        public string Path { get; }

        /// <summary>
        ///     Gets the data binding created using this registration
        /// </summary>
        public DataBinding<TLayerProperty, TProperty> DataBinding { get; internal set; }

        /// <inheritdoc />
        public IDataBinding CreateDataBinding()
        {
            if (DataBinding != null)
                return DataBinding;

            var dataBinding = LayerProperty.Entity.DataBindingEntities.FirstOrDefault(e => e.TargetProperty == Path);
            if (dataBinding == null)
                return null;

            DataBinding = new DataBinding<TLayerProperty, TProperty>(LayerProperty, dataBinding);
            return DataBinding;
        }
    }
}