using System.Drawing;
using Artemis.Utilities;
using Corale.Colore.Razer.Keyboard.Effects;
using Color = Corale.Colore.Core.Color;

namespace Artemis.DeviceProviders.Razer.Utilities
{
    public static class RazerUtilities
    {
        public static Custom BitmapColorArray(Bitmap b, int height, int width)
        {
            var keyboardGrid = Custom.Create();
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
    }
}