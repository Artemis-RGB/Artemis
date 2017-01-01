using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Artemis.Utilities
{
    public class ImageUtilities
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
            var bmp = new RenderTargetBitmap((int) rect.Width, (int) rect.Height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(visual);

            var encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));

            Bitmap bitmap;
            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                bitmap = new Bitmap(stream);
            }

            return bitmap;
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