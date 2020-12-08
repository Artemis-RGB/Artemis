using System;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a generic data binding converter that acts as the bridge between a
    ///     <see cref="DataBinding{TLayerProperty, TProperty}" /> and a <see cref="LayerProperty{T}" /> and does not support
    ///     sum or interpolation
    /// </summary>
    public class GeneralDataBindingConverter<T> : DataBindingConverter<T, T>
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="GeneralDataBindingConverter{T}" /> class
        /// </summary>
        public GeneralDataBindingConverter()
        {
            SupportsSum = false;
            SupportsInterpolate = false;
        }

        /// <inheritdoc />
        public override T Sum(T a, T b)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override T Interpolate(T a, T b, double progress)
        {
            throw new NotSupportedException();
        }
    }
}