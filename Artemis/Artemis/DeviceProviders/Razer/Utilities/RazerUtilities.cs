using System.Drawing;
using Artemis.Utilities;
using Color = Corale.Colore.Core.Color;
using Corale.Colore;

namespace Artemis.DeviceProviders.Razer.Utilities
{
    public static class RazerUtilities
    {
        public static Corale.Colore.Razer.Keyboard.Effects.Custom BitmaptoKeyboardEffect(Bitmap b, int height, int width)
        {

            var keyboardGrid = Corale.Colore.Razer.Keyboard.Effects.Custom.Create();
            // Resize the bitmap
            using (b = ImageUtilities.ResizeImage(b, width, height))
            {
                // Map the bytes to the grid
                for (var x = 0; x < b.Width; x++)
                {
                    for (var y = 0; y < b.Height; y++)
                    {
                        var c = b.GetPixel(x, y);
                        keyboardGrid[y, x] = new Color(c.R, c.G, c.B);
                    }
                }

                return keyboardGrid;
            }
        }

        public static Corale.Colore.Razer.Mousepad.Effects.Custom BitmaptoMousePadEffect(Bitmap b)
        {
            var mousePadGrid = Corale.Colore.Razer.Mousepad.Effects.Custom.Create();
            int pos = 0;
            using (b = ImageUtilities.ResizeImage(b, 5, 5))
            {
                b.RotateFlip(RotateFlipType.RotateNoneFlipX);
                for (var x = 0; x < 5; x++)
                {
                    var c = b.GetPixel(4, x);
                    mousePadGrid[pos++] = new Color(c.R, c.G, c.B);
                }
                for (var x = 0; x < 5; x++)
                {
                    var c = b.GetPixel(x , 4);
                    mousePadGrid[pos++] = new Color(c.R, c.G, c.B);
                }
                for (var x = 0; x < 5; x++)
                {
                    var c = b.GetPixel(0, x);
                    mousePadGrid[pos++] = new Color(c.R, c.G, c.B);
                }
            }
            
            return mousePadGrid;
        }

        public static Corale.Colore.Razer.Mouse.Effects.Custom BitmaptoMouseEffect(Bitmap b)
        {
            var mouseGrid = Corale.Colore.Razer.Mouse.Effects.Custom.Create();
            using (b = ImageUtilities.ResizeImage(b, 3, 7))
            {
                var c = b.GetPixel(1, 0);
                int pos = 0;
                mouseGrid[pos++] = new Color(c.R, c.G, c.B);
                mouseGrid[pos++] = new Color(c.R, c.G, c.B);
                mouseGrid[pos++] = new Color(c.R, c.G, c.B);
                mouseGrid[pos++] = new Color(c.R, c.G, c.B);

                for (var x = 0; x < 7; x++)
                {
                    c = b.GetPixel(0, x);
                    mouseGrid[pos++] = new Color(c.R, c.G, c.B);
                }
                b.RotateFlip(RotateFlipType.RotateNoneFlipX);
                for (var x = 0; x < 7; x++)
                {
                    c = b.GetPixel(2, x);
                    mouseGrid[pos++] = new Color(c.R, c.G, c.B);
                }

            }
            return mouseGrid;
        }
    }
}