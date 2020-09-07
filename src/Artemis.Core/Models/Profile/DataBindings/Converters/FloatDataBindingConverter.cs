using System;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class FloatDataBindingConverter : DataBindingConverter
    {
        public FloatDataBindingConverter()
        {
            SupportedType = typeof(float);
            SupportsSum = true;
            SupportsInterpolate = true;
        }

        /// <inheritdoc />
        public override object Sum(object a, object b)
        {
            return Convert.ToSingle(a) + Convert.ToSingle(b);
        }

        /// <inheritdoc />
        public override object Interpolate(object a, object b, double progress)
        {
            var floatA = Convert.ToSingle(a);
            var floatB = Convert.ToSingle(b);
            var diff = floatB - floatA;
            return floatA + diff * progress;
        }

        /// <inheritdoc />
        public override void ApplyValue(object value)
        {
            var floatValue = Convert.ToSingle(value);
            if (DataBinding.LayerProperty.PropertyDescription.MaxInputValue is float max)
                floatValue = Math.Min(floatValue, max);
            if (DataBinding.LayerProperty.PropertyDescription.MinInputValue is float min)
                floatValue = Math.Max(floatValue, min);

            ValueSetter?.Invoke(floatValue);
        }

        /// <inheritdoc />
        public override object GetValue()
        {
            return ValueGetter?.Invoke();
        }
    }
}