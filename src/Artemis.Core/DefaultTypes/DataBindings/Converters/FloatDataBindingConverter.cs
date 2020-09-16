using System;

namespace Artemis.Core.DefaultTypes
{
    /// <inheritdoc />
    public class FloatDataBindingConverter : FloatDataBindingConverter<float>
    {
    }

    /// <inheritdoc />
    /// <typeparam name="T">The type of layer property this converter is applied to</typeparam>
    public class FloatDataBindingConverter<T> : DataBindingConverter<T, float>
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="FloatDataBindingConverter{T}" /> class
        /// </summary>
        public FloatDataBindingConverter()
        {
            SupportsSum = true;
            SupportsInterpolate = true;
        }

        /// <inheritdoc />
        public override float Sum(float a, float b)
        {
            return a + b;
        }

        /// <inheritdoc />
        public override float Interpolate(float a, float b, double progress)
        {
            var diff = b - a;
            return (float) (a + diff * progress);
        }

        /// <inheritdoc />
        public override void ApplyValue(float value)
        {
            if (ValueTypeSetExpression == null)
                return;

            if (DataBinding.LayerProperty.PropertyDescription.MaxInputValue is float max)
                value = Math.Min(value, max);
            if (DataBinding.LayerProperty.PropertyDescription.MinInputValue is float min)
                value = Math.Max(value, min);

            base.ApplyValue(value);
        }
    }
}