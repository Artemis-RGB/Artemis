using System;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class GeneralDataBindingConverter<T> : DataBindingConverter<T, object> where T : ILayerProperty
    {
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

        /// <inheritdoc />
        public override void ApplyValue(object value)
        {
            ValueSetter?.Invoke(value);
        }

        /// <inheritdoc />
        public override object GetValue()
        {
            return ValueGetter?.Invoke();
        }
    }
}