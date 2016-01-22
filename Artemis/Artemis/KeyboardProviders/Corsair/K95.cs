using System.Drawing;
using CUE.NET;
using CUE.NET.Devices.Generic.Enums;
using CUE.NET.Devices.Keyboard;
using CUE.NET.Brushes;
using CUE.NET.Devices.Keyboard.Keys;

namespace Artemis.KeyboardProviders.Corsair
{
    internal class K95 : KeyboardProvider
    {
        private CorsairKeyboard _keyboard;

        public K95()
        {
            Name = "Corsair Gaming K95 RGB";
        }

        public override void Enable()
        {
            try
            {
                CueSDK.Initialize();
            }
            catch (CUE.NET.Exceptions.WrapperException){/*CUE is already initialized*/}
            _keyboard = CueSDK.KeyboardSDK;
            _keyboard.UpdateMode = UpdateMode.Manual;
            _keyboard.Brush = new SolidColorBrush(Color.Black);
            _keyboard.Update();
        }

        public override void Disable()
        {
        }

        /// <summary>
        /// Properly resizes any size bitmap to the keyboard by creating a rectangle whose size is dependent on the bitmap size.
        /// </summary>
        /// <param name="bitmap"></param>
        public override void DrawBitmap(Bitmap bitmap)
        { 
            RectangleF[,] ledRectangles = new RectangleF[bitmap.Width, bitmap.Height];
            RectangleKeyGroup[,] ledGroups = new RectangleKeyGroup[bitmap.Width, bitmap.Height];

            for (var x = 0 ; x < bitmap.Width; x++)
            {
                for (var y = 0; y < bitmap.Height; y++)
                {
                    ledRectangles[x, y] = new RectangleF(_keyboard.KeyboardRectangle.X * (x*(485F / bitmap.Width / _keyboard.KeyboardRectangle.X)), _keyboard.KeyboardRectangle.Y*(y*(133F / bitmap.Height / _keyboard.KeyboardRectangle.Y)), 485F / bitmap.Width, 133F / bitmap.Height);
                    ledGroups[x, y] = new RectangleKeyGroup(_keyboard, ledRectangles[x, y], 0.1f) { Brush = new SolidColorBrush(bitmap.GetPixel(x, y)) };
                }  
            }
            _keyboard.Update(true);
        }
    }
}