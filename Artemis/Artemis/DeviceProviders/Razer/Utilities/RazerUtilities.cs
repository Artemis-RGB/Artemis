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
            if (b.Width > width || b.Height > height)
                b = ImageUtilities.ResizeImage(b, width, height);

            for (var y = 0; y < b.Height; y++)
            {
                for (var x = 0; x < b.Width; x++)
                {
                    var pixel = b.GetPixel(x, y);
                    keyboardGrid[y, x] = new Color(pixel.R, pixel.G, pixel.B);
                }
            }

            return keyboardGrid;
        }
    }
}