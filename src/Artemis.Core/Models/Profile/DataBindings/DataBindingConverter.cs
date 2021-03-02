using System;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a data binding converter that acts as the bridge between a
    ///     <see cref="DataBinding{TLayerProperty, TProperty}" /> and a <see cref="LayerProperty{T}" />
    /// </summary>
    public abstract class DataBindingConverter<TLayerProperty, TProperty> : IDataBindingConverter
    {
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
            if (DataBinding?.Registration == null)
                throw new ArtemisCoreException("Data binding converter is not yet initialized");
            DataBinding.Registration.Setter(DataBinding.LayerProperty.CurrentValue, value);
        }

        /// <summary>
        ///     Returns the current base value of the data binding
        /// </summary>
        public virtual TProperty GetValue()
        {
            if (DataBinding?.Registration == null)
                throw new ArtemisCoreException("Data binding converter is not yet initialized");
            return DataBinding.Registration.Getter(DataBinding.LayerProperty.CurrentValue);
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
            OnInitialized();
        }

        /// <inheritdoc />
        public Type SupportedType => typeof(TProperty);
    }
}