using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Input;

namespace Artemis.UI.Utilities
{
    public static class CursorUtilities
    {
        public static Cursor GetRotatedCursor(Icon icon, float rotationAngle)
        {
            // Load as Bitmap, convert to BitmapSource
            using (var iconStream = new MemoryStream())
            using (var rotatedStream = new MemoryStream())
            {
                icon.Save(iconStream);

                // Open the source image and create the bitmap for the rotated image
                using (var sourceImage = icon.ToBitmap())
                using (var rotateImage = new Bitmap(sourceImage.Width, sourceImage.Height))
                {
                    // Set the resolution for the rotation image
                    rotateImage.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
                    // Create a graphics object
                    using (var gdi = Graphics.FromImage(rotateImage))
                    {
                        //Rotate the image
                        gdi.TranslateTransform((float) sourceImage.Width / 2, (float) sourceImage.Height / 2);
                        gdi.RotateTransform(rotationAngle);
                        gdi.TranslateTransform(-(float) sourceImage.Width / 2, -(float) sourceImage.Height / 2);
                        gdi.DrawImage(sourceImage, new Point(0, 0));
                    }

                    // Save to a file
                    IconFromImage(rotateImage).Save(rotatedStream);
                }

                // Convert saved file into .cur format
                rotatedStream.Seek(2, SeekOrigin.Begin);
                rotatedStream.Write(iconStream.ToArray(), 2, 1);
                rotatedStream.Seek(10, SeekOrigin.Begin);
                rotatedStream.Write(iconStream.ToArray(), 10, 2);
                rotatedStream.Seek(0, SeekOrigin.Begin);

                // Construct Cursor
                return new Cursor(rotatedStream);
            }
        }

        public static Icon IconFromImage(Image img)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                // Header
                bw.Write((short) 0); // 0 : reserved
                bw.Write((short) 1); // 2 : 1=ico, 2=cur
                bw.Write((short) 1); // 4 : number of images
                // Image directory
                var w = img.Width;
                if (w >= 256) w = 0;
                bw.Write((byte) w); // 0 : width of image
                var h = img.Height;
                if (h >= 256) h = 0;
                bw.Write((byte) h); // 1 : height of image
                bw.Write((byte) 0); // 2 : number of colors in palette
                bw.Write((byte) 0); // 3 : reserved
                bw.Write((short) 0); // 4 : number of color planes
                bw.Write((short) 0); // 6 : bits per pixel
                var sizeHere = ms.Position;
                bw.Write(0); // 8 : image size
                var start = (int) ms.Position + 4;
                bw.Write(start); // 12: offset of image data
                // Image data
                img.Save(ms, ImageFormat.Png);
                var imageSize = (int) ms.Position - start;
                ms.Seek(sizeHere, SeekOrigin.Begin);
                bw.Write(imageSize);
                ms.Seek(0, SeekOrigin.Begin);

                // And load it
                return new Icon(ms);
            }
        }
    }
}