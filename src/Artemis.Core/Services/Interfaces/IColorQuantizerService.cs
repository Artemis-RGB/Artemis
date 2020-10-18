using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Artemis.Core.Services.Interfaces
{
    /// <summary>
    /// A service providing a pallette of colors in a bitmap based on vibrant.js
    /// </summary>
    public interface IColorQuantizerService : IArtemisService
    {
        public SKColor[] Quantize(IEnumerable<SKColor> colors, int amount);

        public SKColor FindColorVariation(IEnumerable<SKColor> colors, ColorType type, bool ignoreLimits = false);
    }
}
