using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Artemis.DeviceProviders.Logitech.Utilities
{
    public static class OrionUtilities
    {
        public static byte[] BitmapToByteArray(Bitmap b, KeyMapping[] keymappings = null)
        {
            if (b.Width > 21 || b.Height > 6)
                b = ResizeImage(b, 21, 6);

            var rect = new Rectangle(0, 0, b.Width, b.Height);
            var bitmapData = b.LockBits(rect, ImageLockMode.ReadWrite, b.PixelFormat);

            var depth = Image.GetPixelFormatSize(b.PixelFormat);
            var step = depth / 8;
            var pixels = new byte[21 * 6 * step];
            var iptr = bitmapData.Scan0;

            // Copy data from pointer to array
            Marshal.Copy(iptr, pixels, 0, pixels.Length);

            if (keymappings == null)
                return pixels;

            var remapped = new byte[pixels.Length];

            // Every  key is 4 bytes
            for (var i = 0; i <= pixels.Length / 4; i++)
            {
                var firstSByte = keymappings[i].Source * 4;
                var firstTByte = keymappings[i].Target * 4;

                for (var j = 0; j < 4; j++)
                    remapped[firstTByte + j] = pixels[firstSByte + j];
            }

            b.Dispose();
            return remapped;
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

                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                // TODO: Make configurable
                // Prevents light bleed
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                // Soft/semi-transparent keys
                //graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public struct KeyMapping
        {
            public KeyMapping(int source, int target)
            {
                Source = source;
                Target = target;
            }

            public int Source { get; set; }
            public int Target { get; set; }
        }
    }
}
