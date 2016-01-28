using System.Drawing;
using CUE.NET;
using CUE.NET.Devices.Generic.Enums;
using CUE.NET.Devices.Keyboard;
using CUE.NET.Brushes;
using CUE.NET.Devices.Keyboard.Keys;

namespace Artemis.KeyboardProviders.Corsair
{
    internal class K70 : KeyboardProvider
    {
        private CorsairKeyboard _keyboard;

        public K70()
        {
            Name = "Corsair Gaming K70 RGB";
        }
        /// <summary>
        /// Enables the SDK and sets updatemode to manual as well as the color of the background to black.
        /// </summary>
        public override void Enable()
        {
            try
            {
                CueSDK.Initialize();
            }
            catch (CUE.NET.Exceptions.WrapperException) {/*CUE is already initialized*/}
            _keyboard = CueSDK.KeyboardSDK;
            _keyboard.UpdateMode = UpdateMode.Manual;
            _keyboard.Brush = new SolidColorBrush(Color.Black);
            _keyboard.Update(true);
        }

        public override void Disable()
        {
        }

        /// <summary>
        /// Properly resizes any size bitmap to the keyboard by creating a rectangle whose size is dependent on the bitmap size.
        /// Does not reset the color each time. Uncomment line 48 for collor reset.
        /// </summary>
        /// <param name="bitmap"></param>
        public override void DrawBitmap(Bitmap bitmap)
        {
            RectangleF[,] ledRectangles = new RectangleF[bitmap.Width, bitmap.Height];
            RectangleKeyGroup[,] ledGroups = new RectangleKeyGroup[bitmap.Width, bitmap.Height];
            //_keyboard.Brush = new SolidColorBrush(Color.Black);
            for (var x = 0; x < bitmap.Width; x++)
            {
                for (var y = 0; y < bitmap.Height; y++)
                {
                    ledRectangles[x, y] = new RectangleF(_keyboard.KeyboardRectangle.X * (x * (_keyboard.KeyboardRectangle.Width / bitmap.Width / _keyboard.KeyboardRectangle.X)), _keyboard.KeyboardRectangle.Y * (y * (_keyboard.KeyboardRectangle.Height / bitmap.Height / _keyboard.KeyboardRectangle.Y)), _keyboard.KeyboardRectangle.Width / bitmap.Width, _keyboard.KeyboardRectangle.Height / bitmap.Height);
                    ledGroups[x, y] = new RectangleKeyGroup(_keyboard, ledRectangles[x, y], 0.01f) { Brush = new SolidColorBrush(bitmap.GetPixel(x, y)) };
                }
            }
            _keyboard.Update();
            for (var x = 0; x < bitmap.Width; x++)
            {
                for (var y = 0; y < bitmap.Height; y++)
                {
                    _keyboard.DetachKeyGroup(ledGroups[x, y]);
                }
            }
        }
    }
}