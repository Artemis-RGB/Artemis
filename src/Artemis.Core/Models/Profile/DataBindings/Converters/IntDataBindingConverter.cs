using System;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class IntDataBindingConverter : DataBindingConverter
    {
        public IntDataBindingConverter()
        {
            SupportedType = typeof(int);
            SupportsSum = true;
            SupportsInterpolate = true;
        }

        /// <summary>
        ///     Gets or sets the <see cref="MidpointRounding" /> mode used for rounding during interpolation. Defaults to
        ///     <see cref="MidpointRounding.AwayFromZero" />
        /// </summary>
        public MidpointRounding InterpolationRoundingMode { get; set; } = MidpointRounding.AwayFromZero;

        /// <inheritdoc />
        public override object Sum(object a, object b)
        {
            return (int) a + (int) b;
        }

        /// <inheritdoc />
        public override object Interpolate(object a, object b, double progress)
        {
            var intA = (int) a;
            var intB = (int) b;
            var diff = intB - intA;
            return (int) Math.Round(intA + diff * progress, InterpolationRoundingMode);
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