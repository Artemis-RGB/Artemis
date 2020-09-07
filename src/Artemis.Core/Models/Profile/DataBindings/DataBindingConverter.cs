using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Artemis.Core
{
    /// <summary>
    ///     A data binding converter that acts as the bridge between a <see cref="DataBinding" /> and a
    ///     <see cref="LayerProperty{T}" />
    /// </summary>
    public abstract class DataBindingConverter
    {
        internal Func<object> ValueGetter { get; set; }
        internal Action<object> ValueSetter { get; set; }

        /// <summary>
        ///     Gets the data binding this converter is applied to
        /// </summary>
        public DataBinding DataBinding { get; private set; }

        /// <summary>
        ///     Gets the type this converter supports
        /// </summary>
        public Type SupportedType { get; protected set; }

        /// <summary>
        ///     Gets whether or not this data binding converter supports the <see cref="Sum" /> method
        /// </summary>
        public bool SupportsSum { get; protected set; }

        /// <summary>
        ///     Gets whether or not this data binding converter supports the <see cref="Interpolate" /> method
        /// </summary>
        public bool SupportsInterpolate { get; protected set; }

        /// <summary>
        ///     Called when the data binding converter has been initialized and the <see cref="DataBinding" /> is available
        /// </summary>
        protected virtual void OnInitialized()
        {
        }

        /// <summary>
        ///     Returns the sum of <paramref name="a" /> and <paramref name="b" />
        /// </summary>
        public abstract object Sum(object a, object b);

        /// <summary>
        ///     Returns the the interpolated value between <paramref name="a" /> and <paramref name="b" /> on a scale (generally)
        ///     between <c>0.0</c> and <c>1.0</c> defined by the <paramref name="progress" />
        ///     <para>Note: The progress may go be negative or go beyond <c>1.0</c> depending on the easing method used</para>
        /// </summary>
        /// <param name="a">The value to interpolate away from</param>
        /// <param name="b">The value to interpolate towards</param>
        /// <param name="progress">The progress of the interpolation between 0.0 and 1.0</param>
        /// <returns></returns>
        public abstract object Interpolate(object a, object b, double progress);

        /// <summary>
        ///     Applies the <paramref name="value" /> to the layer property
        /// </summary>
        /// <param name="value"></param>
        public abstract void ApplyValue(object value);

        /// <summary>
        ///     Returns the current base value of the data binding
        /// </summary>
        public abstract object GetValue();

        internal void Initialize(DataBinding dataBinding)
        {
            DataBinding = dataBinding;
            ValueGetter = CreateValueGetter();
            ValueSetter = CreateValueSetter();

            OnInitialized();
        }

        private Func<object> CreateValueGetter()
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

            // The get method should cast to the object since it receives whatever type the property is
            var body = Expression.Convert(property, typeof(object));
            var lambda = Expression.Lambda<Func<object>>(body);

            return lambda.Compile();
        }

        private Action<object> CreateValueSetter()
        {
            if (DataBinding.TargetProperty?.DeclaringType == null)
                return null;

            var setterMethod = DataBinding.TargetProperty.GetSetMethod();
            if (setterMethod == null)
                return null;

            var constant = Expression.Constant(DataBinding.LayerProperty);
            var propertyValue = Expression.Parameter(typeof(object), "propertyValue");

            // The assign method should cast to the proper type since it receives an object
            var body = Expression.Call(constant, setterMethod, Expression.Convert(propertyValue, DataBinding.TargetProperty.PropertyType));
            var lambda = Expression.Lambda<Action<object>>(body, propertyValue);
            return lambda.Compile();
        }
    }
}