using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Corale.Colore.Razer.Keyboard.Effects;

namespace Artemis.KeyboardProviders.Razer.Utilities
{
    public static class RazerUtilities
    {
        public static Custom BitmapColorArray(Bitmap b, int height, int width)
        {
            var keyboardGrid = Custom.Create();
            if (b.Width > width || b.Height > height)
                b = ResizeImage(b, width, height);

            for (var y = 0; y < b.Height; y++)
                for (var x = 0; x < b.Width; x++)
                    keyboardGrid[y, x] = b.GetPixel(x, y);

            return keyboardGrid;
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