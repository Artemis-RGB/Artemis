using System.Drawing;
using CUE.NET;
using CUE.NET.Devices.Generic.Enums;
using CUE.NET.Devices.Keyboard;

namespace Artemis.KeyboardProviders.Corsair
{
    internal class K70 : KeyboardProvider
    {
        private CorsairKeyboard _keyboard;

        public K70()
        {
            Name = "Corsair Gaming K70 RGB";
        }

        public override void Enable()
        {
            _keyboard = CueSDK.KeyboardSDK;
            _keyboard.UpdateMode = UpdateMode.Manual;
        }

        public override void Disable()
        {
        }

        public override void DrawBitmap(Bitmap bitmap)
        {
            // TODO: Resize bitmap to keyboard's size
            //if (bitmap.Width > width || bitmap.Height > height)
            //    bitmap = ResizeImage(bitmap, width, height);

            // One way of doing this, not sure at all if it's any good
            for (var y = 0; y < bitmap.Height - 1; y++)
                for (var x = 0; x < bitmap.Width - 1; x++)
                    _keyboard[new PointF(x, y)].Led.Color = bitmap.GetPixel(x, y);

            _keyboard.Update(true);
        }
    }
}