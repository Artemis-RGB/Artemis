using System;

namespace Artemis.Profiles.Layers.Types.AmbientLight.Model.Extensions
{
    public static class PixelDataExtension
    {
        #region Methods

        public static int DetectBlackBarLeft(this byte[] pixels, int width, int height, int offsetLeft, int offsetRight,
            int offsetTop, int offsetBottom)
        {
            var bottomBorder = height - offsetBottom;
            var rightBorder = width - offsetRight;

            var blackBarWidth = 0;
            for (var x = rightBorder - 1; x >= offsetLeft; x--)
            {
                for (var y = offsetTop; y < bottomBorder; y++)
                {
                    var offset = (y*width + x)*4;
                    if (pixels[offset] > 15 || pixels[offset + 1] > 15 || pixels[offset + 2] > 15)
                        return blackBarWidth;
                }
                blackBarWidth++;
            }

            return width;
        }

        public static int DetectBlackBarRight(this byte[] pixels, int width, int height, int offsetLeft, int offsetRight,
            int offsetTop, int offsetBottom)
        {
            var bottomBorder = height - offsetBottom;
            var rightBorder = width - offsetRight;

            var blackBarWidth = 0;
            for (var x = offsetLeft; x < rightBorder; x++)
            {
                for (var y = offsetTop; y < bottomBorder; y++)
                {
                    var offset = (y*width + x)*4;
                    if (pixels[offset] > 15 || pixels[offset + 1] > 15 || pixels[offset + 2] > 15)
                        return blackBarWidth;
                }
                blackBarWidth++;
            }

            return width;
        }

        public static int DetectBlackBarTop(this byte[] pixels, int width, int height, int offsetLeft, int offsetRight,
            int offsetTop, int offsetBottom)
        {
            var bottomBorder = height - offsetBottom;
            var rightBorder = width - offsetRight;

            var blackBarHeight = 0;
            for (var y = offsetTop; y < bottomBorder; y++)
            {
                for (var x = offsetLeft; x < rightBorder; x++)
                {
                    var offset = (y*width + x)*4;
                    if (pixels[offset] > 15 || pixels[offset + 1] > 15 || pixels[offset + 2] > 15)
                        return blackBarHeight;
                }
                blackBarHeight++;
            }

            return height;
        }

        public static int DetectBlackBarBottom(this byte[] pixels, int width, int height, int offsetLeft,
            int offsetRight, int offsetTop, int offsetBottom)
        {
            var bottomBorder = height - offsetBottom;
            var rightBorder = width - offsetRight;

            var blackBarHeight = 0;
            for (var y = bottomBorder - 1; y >= offsetTop; y--)
            {
                for (var x = offsetLeft; x < rightBorder; x++)
                {
                    var offset = (y*width + x)*4;
                    if (pixels[offset] > 15 || pixels[offset + 1] > 15 || pixels[offset + 2] > 15)
                        return blackBarHeight;
                }
                blackBarHeight++;
            }

            return height;
        }

        public static byte[] Blend(this byte[] pixels, byte[] blendPixels, SmoothMode smoothMode)
        {
            if (smoothMode == SmoothMode.None || pixels.Length != blendPixels.Length) return blendPixels;

            var percentage = smoothMode == SmoothMode.Low
                ? 0.25
                : (smoothMode == SmoothMode.Medium ? 0.075 : 0.025 /*high*/);

            var blended = new byte[pixels.Length];

            for (var i = 0; i < blended.Length; i++)
                blended[i] = GetIntColor(blendPixels[i]/255.0*percentage + pixels[i]/255.0*(1 - percentage));

            return blended;
        }

        private static byte GetIntColor(double d)
        {
            var calcF = Math.Max(0, Math.Min(1, d));
            return (byte) (calcF.Equals(1) ? 255 : calcF*256);
        }

        #endregion
    }
}