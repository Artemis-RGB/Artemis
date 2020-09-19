using System;

namespace Artemis.Core.DefaultTypes
{
    /// <summary>
    ///     Represents a generic data binding converter that acts as the bridge between a
    ///     <see cref="DataBinding{TLayerProperty, TProperty}" /> and a <see cref="LayerProperty{T}" /> and does not support
    ///     sum or interpolation
    /// </summary>
    public class GeneralDataBindingConverter<T> : DataBindingConverter<T, object> where T : ILayerProperty
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
        public override object Sum(object a, object b)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override object Interpolate(object a, object b, double progress)
        {
            throw new NotSupportedException();
        }
    }
}