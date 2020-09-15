using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class DataBindingRegistration<TLayerProperty, TProperty> : IDataBindingRegistration
    {
        internal DataBindingRegistration(LayerProperty<TLayerProperty> layerProperty,
            DataBindingConverter<TLayerProperty, TProperty> converter,
            Expression<Func<TLayerProperty, TProperty>> propertyExpression)
        {
            LayerProperty = layerProperty ?? throw new ArgumentNullException(nameof(layerProperty));
            Converter = converter ?? throw new ArgumentNullException(nameof(converter));
            PropertyExpression = propertyExpression ?? throw new ArgumentNullException(nameof(propertyExpression));

            if (propertyExpression.Body is MemberExpression memberExpression)
                Member = memberExpression.Member;
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
        ///     Gets the expression that that accesses the property
        /// </summary>
        public Expression<Func<TLayerProperty, TProperty>> PropertyExpression { get; }

        /// <summary>
        ///     Gets the member the <see cref="PropertyExpression" /> targets
        ///     <para><c>null</c> if the <see cref="PropertyExpression" /> is not a member expression</para>
        /// </summary>
        public MemberInfo Member { get; }

        /// <summary>
        ///     Gets the data binding created using this registration
        /// </summary>
        public DataBinding<TLayerProperty, TProperty> DataBinding { get; internal set; }

        /// <inheritdoc />
        public IDataBinding GetDataBinding()
        {
            return DataBinding;
        }
        
        /// <inheritdoc />
        public IDataBinding CreateDataBinding()
        {
            if (DataBinding != null)
                return DataBinding;

            var dataBinding = LayerProperty.Entity.DataBindingEntities.FirstOrDefault(e => e.TargetExpression == PropertyExpression.ToString());
            if (dataBinding == null)
                return null;

            DataBinding = new DataBinding<TLayerProperty, TProperty>(LayerProperty, dataBinding);
            return DataBinding;
        }
    }
}