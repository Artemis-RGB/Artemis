using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Color = Corale.Colore.Core.Color;

namespace Artemis.KeyboardProviders.Razer.Utilities
{
    public static class RazerUtilities
    {
        public static Color[][] BitmapColorArray(Bitmap b, int height, int width)
        {
            var res = new Color[height][];
            if (b.Width > width || b.Height > height)
                b = ResizeImage(b, width, height);

            for (var y = 0; y < b.Height - 1; y++)
            {
                res[y] = new Color[width];
                for (var x = 0; x < b.Width - 1; x++)
                    res[y][x] = b.GetPixel(x, y);
            }
            return res;
        }

        /// <summary>
        ///     Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}