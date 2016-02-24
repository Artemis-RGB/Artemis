using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using Artemis.Utilities;
using CUE.NET;
using CUE.NET.Devices.Generic.Enums;
using CUE.NET.Devices.Keyboard;
using CUE.NET.Exceptions;

namespace Artemis.KeyboardProviders.Corsair
{
    internal class CorsairRGB : KeyboardProvider
    {
        private CorsairKeyboard _keyboard;

        public CorsairRGB()
        {
            Name = "Corsair RGB Keyboards";
            CantEnableText = "Couldn't connect to your Corsair keyboard.\n " +
                             "Please check your cables and/or drivers (could be outdated) and that Corsair Utility Engine is running.\n\n " +
                             "If needed, you can select a different keyboard in Artemis under settings.";
            KeyboardRegions = new List<KeyboardRegion>();
        }

        public override bool CanEnable()
        {
            // Try for about 10 seconds, in case CUE isn't started yet
            var tries = 0;
            while (tries < 9)
            {
                try
                {
                    CueSDK.Initialize();
                }
                catch (CUEException e)
                {
                    if (e.Error == CorsairError.ServerNotFound)
                    {
                        tries++;
                        Thread.Sleep(1000);
                        continue;
                    }
                }
                catch (WrapperException)
                {
                    CueSDK.Reinitialize();
                    return true;
                }

                return true;
            }

            return false;
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

            switch (_keyboard.DeviceInfo.Model)
            {
                case "K95 RGB":
                    Height = 7;
                    Width = 24;
                    KeyboardRegions.Add(new KeyboardRegion("TopRow", new Point(1, 0), new Point(1, 20)));
                    break;
                case "K70 RGB":
                    Height = 7;
                    Width = 21;
                    KeyboardRegions.Add(new KeyboardRegion("TopRow", new Point(1, 0), new Point(1, 16)));
                    break;
                case "K65 RGB":
                    Height = 7;
                    Width = 18;
                    break;
                case "STRAFE RGB":
                    Height = 7;
                    KeyboardRegions.Add(new KeyboardRegion("TopRow", new Point(1, 0), new Point(1, 16)));
                    Width = 22;
                    break;
            }

            // _keyboard.UpdateMode = UpdateMode.Manual;
            _keyboard.Update(true);
        }

        public override void Disable()
        {
            CueSDK.Reinitialize();
        }

        /// <summary>
        ///     Properly resizes any size bitmap to the keyboard by creating a rectangle whose size is dependent on the bitmap
        ///     size.
        ///     Does not reset the color each time. Uncomment line 48 for collor reset.
        /// </summary>
        /// <param name="bitmap"></param>
        public override void DrawBitmap(Bitmap bitmap)
        {
            using (
                var resized = ImageUtilities.ResizeImage(bitmap,
                    (int) _keyboard.KeyboardRectangle.Width,
                    (int) _keyboard.KeyboardRectangle.Height)
                )
            {
                foreach (var item in _keyboard.Keys)
                {
                    var ledColor = resized.GetPixel((int) item.KeyRectangle.X, (int) item.KeyRectangle.Y);
                    if (ledColor == Color.FromArgb(0, 0, 0, 0))
                        ledColor = Color.Black;
                    item.Led.Color = ledColor;
                }
            }
            _keyboard.Update(true);
        }
    }
}