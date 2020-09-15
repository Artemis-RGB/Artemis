using SkiaSharp;

namespace Artemis.Core
{
    /// <inheritdoc />
    public class SKColorDataBindingConverter : DataBindingConverter<SKColor, SKColor>
    {
        /// <inheritdoc />
        public override SKColor Sum(SKColor a, SKColor b)
        {
            return a.Sum(b);
        }

        /// <inheritdoc />
        public override SKColor Interpolate(SKColor a, SKColor b, double progress)
        {
            return a.Interpolate(b, (float)progress);
        }
    }
}
