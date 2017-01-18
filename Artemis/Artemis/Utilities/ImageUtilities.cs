using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Drawing.Point;

namespace Artemis.Utilities
{
    public class ImageUtilities
    {
        private static RenderTargetBitmap _rBmp;

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

        public static BitmapImage BitmapToBitmapImage(Bitmap b)
        {
            if (b == null)
                return null;

            using (var memory = new MemoryStream())
            {
                b.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        public static Bitmap DrawingVisualToBitmap(DrawingVisual visual, Rect rect)
        {
            var width = (int) rect.Width;
            var height = (int) rect.Height;

            // RenderTargetBitmap construction is expensive, only do it when needed
            if (_rBmp?.PixelHeight != height || _rBmp?.PixelWidth != width)
                _rBmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);

            _rBmp.Render(visual);
            return GetBitmap(_rBmp);
        }

        private static Bitmap GetBitmap(BitmapSource source)
        {
            var bmp = new Bitmap(source.PixelWidth, source.PixelHeight, PixelFormat.Format32bppPArgb);
            bmp.SetResolution(96, 96);
            var data = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size), ImageLockMode.WriteOnly,
                PixelFormat.Format32bppPArgb);
            source.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }

        /// <summary>
        ///     Loads the BowIcon from resources and colors it according to the current theme
        /// </summary>
        /// <returns></returns>
        public static RenderTargetBitmap GenerateWindowIcon()
        {
            var iconImage = new System.Windows.Controls.Image
            {
                Source = (DrawingImage) Application.Current.MainWindow.Resources["BowIcon"],
                Stretch = Stretch.Uniform,
                Margin = new Thickness(20)
            };

            iconImage.Arrange(new Rect(0, 0, 100, 100));
            var bitmap = new RenderTargetBitmap(100, 100, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(iconImage);
            return bitmap;
        }

        public static DrawingImage BitmapToDrawingImage(Bitmap b, Rect rect)
        {
            if (b == null)
                return new DrawingImage();

            var visual = new DrawingVisual();
            using (var c = visual.RenderOpen())
            {
                c.DrawImage(BitmapToBitmapImage(b), rect);
            }

            var image = new DrawingImage(visual.Drawing);
            image.Freeze();
            return image;
        }
    }
}