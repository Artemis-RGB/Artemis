using System;

namespace Artemis.Profiles.Layers.Types.AmbientLight.Model.Extensions
{
    public static class PixelDataExtension
    {
        #region Methods

        public static int DetectBlackBarLeft(this byte[] pixels, int width, int height, int offsetLeft, int offsetRight, int offsetTop, int offsetBottom)
        {
            int bottomBorder = height - offsetBottom;
            int rightBorder = width - offsetRight;

            int blackBarWidth = 0;
            for (int x = rightBorder - 1; x >= offsetLeft; x--)
            {
                for (int y = offsetTop; y < bottomBorder; y++)
                {
                    int offset = ((y * width) + x) * 4;
                    if (pixels[offset] > 15 || pixels[offset + 1] > 15 || pixels[offset + 2] > 15)
                        return blackBarWidth;
                }
                blackBarWidth++;
            }

            return width;
        }

        public static int DetectBlackBarRight(this byte[] pixels, int width, int height, int offsetLeft, int offsetRight, int offsetTop, int offsetBottom)
        {
            int bottomBorder = height - offsetBottom;
            int rightBorder = width - offsetRight;

            int blackBarWidth = 0;
            for (int x = offsetLeft; x < rightBorder; x++)
            {
                for (int y = offsetTop; y < bottomBorder; y++)
                {
                    int offset = ((y * width) + x) * 4;
                    if (pixels[offset] > 15 || pixels[offset + 1] > 15 || pixels[offset + 2] > 15)
                        return blackBarWidth;
                }
                blackBarWidth++;
            }

            return width;
        }

        public static int DetectBlackBarTop(this byte[] pixels, int width, int height, int offsetLeft, int offsetRight, int offsetTop, int offsetBottom)
        {
            int bottomBorder = height - offsetBottom;
            int rightBorder = width - offsetRight;

            int blackBarHeight = 0;
            for (int y = offsetTop; y < bottomBorder; y++)
            {
                for (int x = offsetLeft; x < rightBorder; x++)
                {
                    int offset = ((y * width) + x) * 4;
                    if (pixels[offset] > 15 || pixels[offset + 1] > 15 || pixels[offset + 2] > 15)
                        return blackBarHeight;
                }
                blackBarHeight++;
            }

            return height;
        }

        public static int DetectBlackBarBottom(this byte[] pixels, int width, int height, int offsetLeft, int offsetRight, int offsetTop, int offsetBottom)
        {
            int bottomBorder = height - offsetBottom;
            int rightBorder = width - offsetRight;

            int blackBarHeight = 0;
            for (int y = bottomBorder - 1; y >= offsetTop; y--)
            {
                for (int x = offsetLeft; x < rightBorder; x++)
                {
                    int offset = ((y * width) + x) * 4;
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
            
            double percentage = smoothMode == SmoothMode.Low? 0.25: (smoothMode == SmoothMode.Medium ? 0.075 : 0.025 /*high*/);

            byte[] blended = new byte[pixels.Length];

            for (int i = 0; i < blended.Length; i++)
                blended[i] = GetIntColor((blendPixels[i] / 255.0) * percentage + (pixels[i] / 255.0) * (1 - percentage));

            return blended;
        }

        private static byte GetIntColor(double d)
        {
            double calcF = Math.Max(0, Math.Min(1, d));
            return (byte)(calcF.Equals(1) ? 255 : calcF * 256);
        }

        #endregion
    }
}
