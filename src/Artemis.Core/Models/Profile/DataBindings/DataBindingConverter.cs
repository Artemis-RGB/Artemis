using System;
using System.Linq;
using System.Linq.Expressions;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a data binding converter that acts as the bridge between a
    ///     <see cref="DataBinding{TLayerProperty, TProperty}" /> and a <see cref="LayerProperty{T}" />
    /// </summary>
    public abstract class DataBindingConverter<TLayerProperty, TProperty> : IDataBindingConverter
    {
        /// <summary>
        ///     A dynamically compiled getter pointing to the data bound property
        /// </summary>
        public Func<TProperty> ValueGetter { get; private set; }

        /// <summary>
        ///     A dynamically compiled setter pointing to the data bound property
        /// </summary>
        public Action<TProperty> ValueSetter { get; private set; }

        /// <summary>
        ///     Gets the data binding this converter is applied to
        /// </summary>
        public DataBinding<TLayerProperty, TProperty> DataBinding { get; private set; }

        /// <summary>
        ///     Gets whether or not this data binding converter supports the <see cref="Sum" /> method
        /// </summary>
        public bool SupportsSum { get; protected set; }

        /// <summary>
        ///     Gets whether or not this data binding converter supports the <see cref="Interpolate" /> method
        /// </summary>
        public bool SupportsInterpolate { get; protected set; }

        /// <inheritdoc />
        public Type SupportedType => typeof(TProperty);

        /// <summary>
        ///     Returns the sum of <paramref name="a" /> and <paramref name="b" />
        /// </summary>
        public abstract TProperty Sum(TProperty a, TProperty b);

        /// <summary>
        ///     Returns the the interpolated value between <paramref name="a" /> and <paramref name="b" /> on a scale (generally)
        ///     between <c>0.0</c> and <c>1.0</c> defined by the <paramref name="progress" />
        ///     <para>Note: The progress may go be negative or go beyond <c>1.0</c> depending on the easing method used</para>
        /// </summary>
        /// <param name="a">The value to interpolate away from</param>
        /// <param name="b">The value to interpolate towards</param>
        /// <param name="progress">The progress of the interpolation between 0.0 and 1.0</param>
        /// <returns></returns>
        public abstract TProperty Interpolate(TProperty a, TProperty b, double progress);

        /// <summary>
        ///     Applies the <paramref name="value" /> to the layer property
        /// </summary>
        /// <param name="value"></param>
        public abstract void ApplyValue(TProperty value);

        /// <summary>
        ///     Returns the current base value of the data binding
        /// </summary>
        public abstract TProperty GetValue();

        /// <summary>
        ///     Called when the data binding converter has been initialized and the <see cref="DataBinding" /> is available
        /// </summary>
        protected virtual void OnInitialized()
        {
        }

        internal void Initialize(DataBinding<TLayerProperty, TProperty> dataBinding)
        {
            DataBinding = dataBinding;
            ValueGetter = CreateValueGetter();
            ValueSetter = CreateValueSetter();

            OnInitialized();
        }

        private Func<TProperty> CreateValueGetter()
        {
            if (DataBinding.TargetProperty?.DeclaringType == null)
                return null;

            var getterMethod = DataBinding.TargetProperty.GetGetMethod();
            if (getterMethod == null)
                return null;

            var constant = Expression.Constant(DataBinding.LayerProperty);
            // The path is null if the registration is applied to the root (LayerProperty.CurrentValue)
            var property = DataBinding.Registration.Path == null
                ? Expression.Property(constant, DataBinding.TargetProperty)
                : (MemberExpression) DataBinding.Registration.Path.Split('.').Aggregate<string, Expression>(constant, Expression.Property);

            var lambda = Expression.Lambda<Func<TProperty>>(property);

            return lambda.Compile();
        }

        private Action<TProperty> CreateValueSetter()
        {
            if (DataBinding.TargetProperty?.DeclaringType == null)
                return null;

            var setterMethod = DataBinding.TargetProperty.GetSetMethod();
            if (setterMethod == null)
                return null;

            var constant = Expression.Constant(DataBinding.LayerProperty);
            var propertyValue = Expression.Parameter(typeof(TProperty), "propertyValue");

            var body = Expression.Call(constant, setterMethod, propertyValue);
            var lambda = Expression.Lambda<Action<TProperty>>(body, propertyValue);
            return lambda.Compile();
        }
    }
}