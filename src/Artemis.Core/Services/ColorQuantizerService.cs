using Artemis.Core.Services.Interfaces;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Artemis.Core.Services
{
    public enum ColorType
    {
        Vibrant,
        LightVibrant,
        DarkVibrant,
        Muted,
        LightMuted,
        DarkMuted
    }

    public class ColorQuantizerService : IColorQuantizerService
    {
        #region Quantizer
        public SKColor[] Quantize(IEnumerable<SKColor> colors, int amount)
        {
            if ((amount & (amount - 1)) != 0)
                throw new ArgumentException("Must be power of two", nameof(amount));

            Queue<Cube> cubes = new Queue<Cube>(amount);
            cubes.Enqueue(new Cube(colors));

            while (cubes.Count < amount)
            {
                Cube cube = cubes.Dequeue();
                if (cube.TrySplit(out var a, out var b))
                {
                    cubes.Enqueue(a);
                    cubes.Enqueue(b);
                }
            }

            return cubes.Select(c => c.GetAverageColor()).ToArray();
        }

        private class Cube
        {
            private readonly List<SKColor> _colors;

            internal Cube(IEnumerable<SKColor> colors)
            {
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

            internal bool TrySplit([NotNullWhen(returnValue: true)] out Cube? a, [NotNullWhen(returnValue: true)] out Cube? b)
            {
                if (_colors.Count < 2)
                {
                    a = null;
                    b = null;
                    return false;
                }

                int median = _colors.Count / 2;

                a = new Cube(_colors.GetRange(0, median));
                b = new Cube(_colors.GetRange(median, _colors.Count - median));

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
        #endregion

        #region Vibrant Classifier
        private const float targetDarkLuma = 0.26f;
        private const float maxDarkLuma = 0.45f;
        private const float minLightLuma = 0.55f;
        private const float targetLightLuma = 0.74f;
        private const float minNormalLuma = 0.3f;
        private const float targetNormalLuma = 0.5f;
        private const float maxNormalLuma = 0.7f;
        private const float targetMutesSaturation = 0.3f;
        private const float maxMutesSaturation = 0.3f;
        private const float targetVibrantSaturation = 1.0f;
        private const float minVibrantSaturation = 0.35f;
        private const float weightSaturation = 3f;
        private const float weightLuma = 5f;

        public SKColor FindColorVariation(IEnumerable<SKColor> colors, ColorType type, bool ignoreLimits = false)
        {
            (float targetLuma, float minLuma, float maxLuma, float targetSaturation, float minSaturation, float maxSaturation) = type switch
            {
                ColorType.Vibrant => (targetNormalLuma, minNormalLuma, maxNormalLuma, targetVibrantSaturation, minVibrantSaturation, 1f),
                ColorType.LightVibrant => (targetLightLuma, minLightLuma, 1f, targetVibrantSaturation, minVibrantSaturation, 1f),
                ColorType.DarkVibrant => (targetDarkLuma, 0f, maxDarkLuma, targetVibrantSaturation, minVibrantSaturation, 1f),
                ColorType.Muted => (targetNormalLuma, minNormalLuma, maxNormalLuma, targetMutesSaturation, 0, maxMutesSaturation),
                ColorType.LightMuted => (targetLightLuma, minLightLuma, 1f, targetMutesSaturation, 0, maxMutesSaturation),
                ColorType.DarkMuted => (targetDarkLuma, 0, maxDarkLuma, targetMutesSaturation, 0, maxMutesSaturation),
                _ => (0.5f, 0f, 1f, 0.5f, 0f, 1f)
            };

            var bestColorScore = float.MinValue;
            var bestColor = SKColor.Empty;
            foreach (var clr in colors)
            {
                clr.ToHsl(out float _, out float sat, out float luma);
                sat /= 100f;
                luma /= 100f;

                if (!ignoreLimits && (sat <= minSaturation || sat >= maxSaturation || luma <= minLuma || luma >= maxLuma))
                    continue;

                var score = GetComparisonValue(sat, targetSaturation, luma, targetLuma);
                if (score > bestColorScore)
                {
                    bestColorScore = score;
                    bestColor = clr;
                }
            }

            return bestColor;
        }

        private static float GetComparisonValue(float sat, float targetSaturation, float luma, float targetLuma)
        {
            static float InvertDiff(float value, float target) => 1 - Math.Abs(value - target);
            const float totalweight = weightSaturation + weightLuma;

            float totalValue = (InvertDiff(sat, targetSaturation) * weightSaturation) +
                               (InvertDiff(luma, targetLuma) * weightLuma);

            return totalValue / totalweight;
        }
        #endregion
    }
}