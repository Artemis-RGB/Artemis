using System;

namespace Artemis.Core.DefaultTypes
{
    /// <inheritdoc />
    public class IntDataBindingConverter : IntDataBindingConverter<int>
    {
    }

    /// <inheritdoc />
    public class IntDataBindingConverter<T> : DataBindingConverter<T, int>
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
            int diff = b - a;
            return (int) Math.Round(a + diff * progress, InterpolationRoundingMode);
        }
    }
}