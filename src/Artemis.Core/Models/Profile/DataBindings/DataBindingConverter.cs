using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a data binding converter that acts as the bridge between a
    ///     <see cref="DataBinding{TLayerProperty, TProperty}" /> and a <see cref="LayerProperty{T}" />
    /// </summary>
    public abstract class DataBindingConverter<TLayerProperty, TProperty> : IDataBindingConverter
    {
        /// <summary>
        ///     Gets a dynamically compiled getter pointing to the data bound property
        /// </summary>
        public Func<TLayerProperty, TProperty>? GetExpression { get; private set; }

        /// <summary>
        ///     Gets a dynamically compiled setter pointing to the data bound property used for value types
        /// </summary>
        public Action<TProperty>? ValueTypeSetExpression { get; private set; }

        /// <summary>
        ///     Gets a dynamically compiled setter pointing to the data bound property used for reference types
        /// </summary>
        public Action<TLayerProperty, TProperty>? ReferenceTypeSetExpression { get; private set; }

        /// <summary>
        ///     Gets the data binding this converter is applied to
        /// </summary>
        public DataBinding<TLayerProperty, TProperty>? DataBinding { get; private set; }

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
        public virtual void ApplyValue(TProperty value)
        {
            if (DataBinding == null)
                throw new ArtemisCoreException("Data binding converter is not yet initialized");
            if (ReferenceTypeSetExpression != null)
                ReferenceTypeSetExpression(DataBinding.LayerProperty.CurrentValue, value);
            else if (ValueTypeSetExpression != null)
                ValueTypeSetExpression(value);
        }

        /// <summary>
        ///     Returns the current base value of the data binding
        /// </summary>
        public virtual TProperty GetValue()
        {
            if (DataBinding == null || GetExpression == null)
                throw new ArtemisCoreException("Data binding converter is not yet initialized");
            return GetExpression(DataBinding.LayerProperty.CurrentValue);
        }

        /// <summary>
        ///     Converts the provided object to a type of <typeparamref name="TProperty" />
        /// </summary>
        public virtual TProperty ConvertFromObject(object? source)
        {
            return (TProperty) Convert.ChangeType(source, typeof(TProperty))!;
        }

        /// <summary>
        ///     Called when the data binding converter has been initialized and the <see cref="DataBinding" /> is available
        /// </summary>
        protected virtual void OnInitialized()
        {
        }

        internal void Initialize(DataBinding<TLayerProperty, TProperty> dataBinding)
        {
            if (dataBinding.Registration == null)
                throw new ArtemisCoreException("Cannot initialize a data binding converter for a data binding without a registration");

            DataBinding = dataBinding;
            GetExpression = dataBinding.Registration.PropertyExpression.Compile();
            CreateSetExpression();

            OnInitialized();
        }

        private void CreateSetExpression()
        {
            // If the registration does not point towards a member of LayerProperty<T>.CurrentValue, assign directly to LayerProperty<T>.CurrentValue
            if (DataBinding!.Registration?.Member == null)
            {
                CreateSetCurrentValueExpression();
                return;
            }

            // Ensure the member of LayerProperty<T>.CurrentValue has a setter
            MethodInfo? setterMethod = null;
            if (DataBinding.Registration.Member is PropertyInfo propertyInfo)
                setterMethod = propertyInfo.GetSetMethod();
            // If there is no setter, the built-in data binding cannot do its job, stay null
            if (setterMethod == null)
                return;

            // If LayerProperty<T>.CurrentValue is a value type, assign it directly to LayerProperty<T>.CurrentValue after applying the changes
            if (typeof(TLayerProperty).IsValueType)
                CreateSetValueTypeExpression();
            // If it is a reference type it can safely be updated by its reference
            else
                CreateSetReferenceTypeExpression();
        }

        private void CreateSetReferenceTypeExpression()
        {
            if (DataBinding!.Registration?.Member == null)
                throw new ArtemisCoreException("Cannot create value setter for data binding without a registration");

            ParameterExpression propertyValue = Expression.Parameter(typeof(TProperty), "propertyValue");
            ParameterExpression parameter = Expression.Parameter(typeof(TLayerProperty), "currentValue");
            MemberExpression memberAccess = Expression.MakeMemberAccess(parameter, DataBinding.Registration.Member);
            BinaryExpression assignment = Expression.Assign(memberAccess, propertyValue);
            Expression<Action<TLayerProperty, TProperty>> referenceTypeLambda = Expression.Lambda<Action<TLayerProperty, TProperty>>(assignment, parameter, propertyValue);

            ReferenceTypeSetExpression = referenceTypeLambda.Compile();
        }

        private void CreateSetValueTypeExpression()
        {
            if (DataBinding!.Registration?.Member == null)
                throw new ArtemisCoreException("Cannot create value setter for data binding without a registration");

            ParameterExpression propertyValue = Expression.Parameter(typeof(TProperty), "propertyValue");
            ParameterExpression variableCurrent = Expression.Variable(typeof(TLayerProperty), "current");
            ConstantExpression layerProperty = Expression.Constant(DataBinding.LayerProperty);
            MemberExpression layerPropertyMemberAccess = Expression.MakeMemberAccess(layerProperty,
                DataBinding.LayerProperty.GetType().GetMember(nameof(DataBinding.LayerProperty.CurrentValue))[0]);

            BlockExpression body = Expression.Block(
                new[] {variableCurrent},
                Expression.Assign(variableCurrent, layerPropertyMemberAccess),
                Expression.Assign(Expression.MakeMemberAccess(variableCurrent, DataBinding.Registration.Member), propertyValue),
                Expression.Assign(layerPropertyMemberAccess, variableCurrent)
            );

            Expression<Action<TProperty>> valueTypeLambda = Expression.Lambda<Action<TProperty>>(body, propertyValue);
            ValueTypeSetExpression = valueTypeLambda.Compile();
        }

        private void CreateSetCurrentValueExpression()
        {
            ParameterExpression propertyValue = Expression.Parameter(typeof(TProperty), "propertyValue");
            ConstantExpression layerProperty = Expression.Constant(DataBinding!.LayerProperty);
            MemberExpression layerPropertyMemberAccess = Expression.MakeMemberAccess(layerProperty,
                DataBinding.LayerProperty.GetType().GetMember(nameof(DataBinding.LayerProperty.CurrentValue))[0]);

            BinaryExpression body = Expression.Assign(layerPropertyMemberAccess, propertyValue);
            Expression<Action<TProperty>> lambda = Expression.Lambda<Action<TProperty>>(body, propertyValue);
            ValueTypeSetExpression = lambda.Compile();
        }
    }
}