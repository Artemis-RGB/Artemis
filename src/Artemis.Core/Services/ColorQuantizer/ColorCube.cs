using SkiaSharp;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Artemis.Core.Services
{
    internal class ColorCube
    {
        private readonly List<SKColor> _colors;

        internal ColorCube(IEnumerable<SKColor> colors)
        {
            if (colors.Count() < 2)
            {
                _colors = colors.ToList();
                return;
            }

            int redRange = colors.Max(c => c.Red) - colors.Min(c => c.Red);
            int greenRange = colors.Max(c => c.Green) - colors.Min(c => c.Green);
            int blueRange = colors.Max(c => c.Blue) - colors.Min(c => c.Blue);

            if (redRange > greenRange && redRange > blueRange)
                _colors = colors.OrderBy(a => a.Red).ToList();
            else if (greenRange > blueRange)
                _colors = colors.OrderBy(a => a.Green).ToList();
            else
                _colors = colors.OrderBy(a => a.Blue).ToList();
        }

        internal bool TrySplit([NotNullWhen(returnValue: true)] out ColorCube? a, [NotNullWhen(returnValue: true)] out ColorCube? b)
        {
            if (_colors.Count < 2)
            {
                a = null;
                b = null;
                return false;
            }

            int median = _colors.Count / 2;

            a = new ColorCube(_colors.GetRange(0, median));
            b = new ColorCube(_colors.GetRange(median, _colors.Count - median));

            return true;
        }

        internal SKColor GetAverageColor()
        {
            int r = 0, g = 0, b = 0;

            for (int i = 0; i < _colors.Count; i++)
            {
                r += _colors[i].Red;
                g += _colors[i].Green;
                b += _colors[i].Blue;
            }

            return new SKColor(
                (byte)(r / _colors.Count),
                (byte)(g / _colors.Count),
                (byte)(b / _colors.Count)
            );
        }
    }
}