using SkiaSharp;

namespace Artemis.Core.DefaultTypes
{
    /// <inheritdoc />
    public class SKColorDataBindingConverter : DataBindingConverter<SKColor, SKColor>
    {
        public SKColorDataBindingConverter()
        {
            SupportsInterpolate = true;
            SupportsSum = true;
        }

        /// <inheritdoc />
        public override SKColor Sum(SKColor a, SKColor b)
        {
            return a.Sum(b);
        }

        /// <inheritdoc />
        public override SKColor Interpolate(SKColor a, SKColor b, double progress)
        {
            return a.Interpolate(b, (float) progress);
        }
    }
}