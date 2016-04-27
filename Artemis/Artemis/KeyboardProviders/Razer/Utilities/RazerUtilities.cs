using System.Drawing;
using Artemis.Utilities;
using Corale.Colore.Razer.Keyboard.Effects;

namespace Artemis.KeyboardProviders.Razer.Utilities
{
    public static class RazerUtilities
    {
        public static Custom BitmapColorArray(Bitmap b, int height, int width)
        {
            var keyboardGrid = Custom.Create();
            if (b.Width > width || b.Height > height)
                b = ImageUtilities.ResizeImage(b, width, height);

            for (var y = 0; y < b.Height; y++)
                for (var x = 0; x < b.Width; x++)
                    keyboardGrid[y, x] = b.GetPixel(x, y);

            return keyboardGrid;
        }
    }
}