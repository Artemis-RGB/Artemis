using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace Artemis.Core.Services
{
    /// <inheritdoc />
    internal class ColorQuantizerService : IColorQuantizerService
    {
        private static float GetComparisonValue(float sat, float targetSaturation, float luma, float targetLuma)
        {
            static float InvertDiff(float value, float target)
            {
                return 1 - Math.Abs(value - target);
            }

            const float totalWeight = weightSaturation + weightLuma;

            float totalValue = InvertDiff(sat, targetSaturation) * weightSaturation +
                               InvertDiff(luma, targetLuma) * weightLuma;

            return totalValue / totalWeight;
        }

        /// <inheritdoc />
        public SKColor[] Quantize(IEnumerable<SKColor> colors, int amount)
        {
            if ((amount & (amount - 1)) != 0)
                throw new ArgumentException("Must be power of two", nameof(amount));

            Queue<ColorCube> cubes = new(amount);
            cubes.Enqueue(new ColorCube(colors));

            while (cubes.Count < amount)
            {
                ColorCube cube = cubes.Dequeue();
                if (cube.TrySplit(out ColorCube? a, out ColorCube? b))
                {
                    cubes.Enqueue(a);
                    cubes.Enqueue(b);
                }
            }

            return cubes.Select(c => c.GetAverageColor()).ToArray();
        }

        /// <inheritdoc />
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

            float bestColorScore = float.MinValue;
            SKColor bestColor = SKColor.Empty;
            foreach (SKColor clr in colors)
            {
                clr.ToHsl(out float _, out float sat, out float luma);
                sat /= 100f;
                luma /= 100f;

                if (!ignoreLimits && (sat <= minSaturation || sat >= maxSaturation || luma <= minLuma || luma >= maxLuma))
                    continue;

                float score = GetComparisonValue(sat, targetSaturation, luma, targetLuma);
                if (score > bestColorScore)
                {
                    bestColorScore = score;
                    bestColor = clr;
                }
            }

            return bestColor;
        }

        #region Constants

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

        #endregion
    }
}