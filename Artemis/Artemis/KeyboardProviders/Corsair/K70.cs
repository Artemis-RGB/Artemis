using System;
using System.Drawing;
using System.Windows.Forms;
using Artemis.Utilities;
using CUE.NET;
using CUE.NET.Devices.Generic.Enums;
using CUE.NET.Devices.Keyboard;
using CUE.NET.Exceptions;

namespace Artemis.KeyboardProviders.Corsair
{
    internal class K70 : KeyboardProvider
    {
        private CorsairKeyboard _keyboard;

        public K70()
        {
            Name = "Corsair Gaming K70 RGB";
        }

        public override bool CanEnable()
        {
            try
            {
                CueSDK.Initialize();
            }
            catch (CUEException e)
            {
                if (e.Error == CorsairError.ServerNotFound)
                    return false;
                throw;
            }

            return true;

        }

        /// <summary>
        ///     Enables the SDK and sets updatemode to manual as well as the color of the background to black.
        /// </summary>
        public override void Enable()
        {
            try
            {
                CueSDK.Initialize();
            }
            catch (WrapperException)
            {
                /*CUE is already initialized*/
            }
            _keyboard = CueSDK.KeyboardSDK;
            Height = (int)_keyboard.KeyboardRectangle.Height;
            Width = (int)_keyboard.KeyboardRectangle.Width;

            // _keyboard.UpdateMode = UpdateMode.Manual;
            _keyboard.Update(true);
        }

        public override void Disable()
        {
        }

        /// <summary>
        ///     Properly resizes any size bitmap to the keyboard by creating a rectangle whose size is dependent on the bitmap
        ///     size.
        ///     Does not reset the color each time. Uncomment line 48 for collor reset.
        /// </summary>
        /// <param name="bitmap//"></param>
        public override void DrawBitmap(Bitmap bitmap)
        {
            using (
                var resized = ImageUtilities.ResizeImage(bitmap,
                    (int)_keyboard.KeyboardRectangle.Width,
                    (int)_keyboard.KeyboardRectangle.Height)
                )
            {
                foreach (var item in _keyboard.Keys)
                {
                    var ledColor = resized.GetPixel((int)item.KeyRectangle.X, (int)item.KeyRectangle.Y);
                    if (ledColor == Color.FromArgb(0, 0, 0, 0))
                        ledColor = Color.Black;
                    item.Led.Color = ledColor;
                }
            }
            _keyboard.Update(true);
        }
    }
}