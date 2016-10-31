namespace Artemis.Profiles.Layers.Types.AmbientLight.Model.Extensions
{
    public static class PixelDataExtension
    {
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
    }
}
