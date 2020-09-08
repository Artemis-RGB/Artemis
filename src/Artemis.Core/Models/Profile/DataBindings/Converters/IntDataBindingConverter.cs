using System;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class IntDataBindingConverter : FloatDataBindingConverter<int>
    {
    }

    /// <inheritdoc />
    public class IntDataBindingConverter<T> : DataBindingConverter<T, int> where T : ILayerProperty
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="IntDataBindingConverter{T}" /> class
        /// </summary>
        public IntDataBindingConverter()
        {
            SupportsSum = true;
            SupportsInterpolate = true;
        }

        /// <summary>
        ///     Gets or sets the <see cref="MidpointRounding" /> mode used for rounding during interpolation. Defaults to
        ///     <see cref="MidpointRounding.AwayFromZero" />
        /// </summary>
        public MidpointRounding InterpolationRoundingMode { get; set; } = MidpointRounding.AwayFromZero;

        /// <inheritdoc />
        public override int Sum(int a, int b)
        {
            return a + b;
        }

        /// <inheritdoc />
        public override int Interpolate(int a, int b, double progress)
        {
            var diff = b - a;
            return (int) Math.Round(a + diff * progress, InterpolationRoundingMode);
        }

        /// <inheritdoc />
        public override void ApplyValue(int value)
        {
            ValueSetter?.Invoke(value);
        }

        /// <inheritdoc />
        public override int GetValue()
        {
            return ValueGetter?.Invoke() ?? 0;
        }
    }
}