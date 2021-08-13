using SkiaSharp;
using System.Collections.Generic;

namespace Artemis.Core.Services
{
    /// <summary>
    /// A service providing a pallette of colors in a bitmap based on vibrant.js
    /// </summary>
    public interface IColorQuantizerService : IArtemisService
    {
        /// <summary>
        /// Reduces an <see cref="IEnumerable{SKColor}"/> to a given amount of relevant colors. Based on the Median Cut algorithm
        /// </summary>
        /// <param name="colors">The colors to quantize.</param>
        /// <param name="amount">The number of colors that should be calculated. Must be a power of two.</param>
        /// <returns>The quantized colors.</returns>
        public SKColor[] Quantize(IEnumerable<SKColor> colors, int amount);

        /// <summary>
        /// Finds colors with certain characteristics in a given <see cref="IEnumerable{SKColor}"/>.<para />
        /// Vibrant variants are more saturated, while Muted colors are less.<para />
        /// Light and Dark colors have higher and lower lightness values, respectively.
        /// </summary>
        /// <param name="colors">The colors to find the variations in</param>
        /// <param name="type">Which type of color to find</param>
        /// <param name="ignoreLimits">Ignore hard limits on whether a color is considered for each category. Result may be <see cref="SKColor.Empty"/> if this is false</param>
        /// <returns>The color found</returns>
        public SKColor FindColorVariation(IEnumerable<SKColor> colors, ColorType type, bool ignoreLimits = false);

        /// <summary>
        /// Finds all the color variations available and returns a struct containing them all.
        /// </summary>
        /// <param name="colors">The colors to find the variations in</param>
        /// <param name="ignoreLimits">Ignore hard limits on whether a color is considered for each category. Some colors may be <see cref="SKColor.Empty"/> if this is false</param>
        /// <returns>A swatch containing all color variations</returns>
        public ColorSwatch FindAllColorVariations(IEnumerable<SKColor> colors, bool ignoreLimits = false);
    }
}
