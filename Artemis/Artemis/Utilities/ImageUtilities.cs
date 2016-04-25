using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Artemis.Utilities
{
    internal class ImageUtilities
    {
        /// <summary>
        ///     Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            //a holder for the result
            var result = new Bitmap(width, height);
            //set the resolutions the same to avoid cropping due to resolution differences
            result.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            //use a graphics object to draw the resized image into the bitmap
            using (var graphics = Graphics.FromImage(result))
            {
                //draw the image into the target bitmap
                graphics.DrawImage(image, 0, 0, result.Width, result.Height);
            }

            //return the resulting bitmap
            return result;
        }

        public static Bitmap BitmapSourceToBitmap(BitmapSource srs)
        {
            var width = srs.PixelWidth;
            var height = srs.PixelHeight;
            var stride = width*((srs.Format.BitsPerPixel + 7)/8);
            var ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(height*stride);
                srs.CopyPixels(new Int32Rect(0, 0, width, height), ptr, height*stride, stride);
                using (var btm = new Bitmap(width, height, stride, PixelFormat.Format1bppIndexed, ptr))
                {
                    // Clone the bitmap so that we can dispose it and
                    // release the unmanaged memory at ptr
                    return new Bitmap(btm);
                }
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }

        public static Bitmap DrawinVisualToBitmap(DrawingVisual visual, Rect rect)
        {
            var bmp = new RenderTargetBitmap((int) rect.Width, (int) rect.Height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(visual);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));

            Bitmap bitmap;
            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                bitmap = new Bitmap(stream);
            }
            return bitmap;
        }
    }
}