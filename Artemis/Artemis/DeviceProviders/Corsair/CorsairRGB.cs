using System;
using System.Drawing;
using System.Threading;
using System.Windows;
using Artemis.Properties;
using Artemis.Utilities;
using CUE.NET;
using CUE.NET.Brushes;
using CUE.NET.Devices.Keyboard;

namespace Artemis.DeviceProviders.Corsair
{
    public class CorsairRGB : KeyboardProvider
    {
        private CorsairKeyboard _keyboard;

        public CorsairRGB()
        {
            Name = "Corsair RGB Keyboards";
            CantEnableText = "Couldn't connect to your Corsair keyboard.\n" +
                             "Please check your cables and/or drivers (could be outdated) and that Corsair Utility Engine is running.\n" +
                             "In CUE, make sure \"Enable SDK\" is checked under Settings > Program.\n\n" +
                             "If needed, you can select a different keyboard in Artemis under settings.";
        }

        public override bool CanEnable()
        {
            // If already initialized, return result right away
            if (CueSDK.ProtocolDetails != null)
                return CueSDK.KeyboardSDK != null;

            // Else try to enable the SDK
            for (var tries = 0; tries < 9; tries++)
            {
                if (CueSDK.ProtocolDetails != null)
                    break;

                try
                {
                    CueSDK.Initialize();
                }
                catch (Exception)
                {
                    Thread.Sleep(2000);
                }
            }

            if (CueSDK.ProtocolDetails == null)
                return false;
            return CueSDK.KeyboardSDK != null;
        }

        /// <summary>
        ///     Enables the SDK and sets updatemode to manual as well as the color of the background to black.
        /// </summary>
        public override void Enable()
        {
            if (CueSDK.ProtocolDetails == null)
                CueSDK.Initialize();

            _keyboard = CueSDK.KeyboardSDK;
            switch (_keyboard.DeviceInfo.Model)
            {
                case "K95 RGB":
                    Height = 7;
                    Width = 25;
                    PreviewSettings = new PreviewSettings(676, 190, new Thickness(0, -15, 0, 0), Resources.k95);
                    break;
                case "K70 RGB":
                    Height = 7;
                    Width = 21;
                    PreviewSettings = new PreviewSettings(676, 195, new Thickness(0, -25, 0, 0), Resources.k70);
                    break;
                case "K65 RGB":
                    Height = 7;
                    Width = 18;
                    PreviewSettings = new PreviewSettings(660, 240, new Thickness(0, -30, 0, 0), Resources.k65);
                    break;
                case "STRAFE RGB":
                    Height = 6;
                    Width = 22;
                    PreviewSettings = new PreviewSettings(670, 215, new Thickness(0, -15, 0, 0), Resources.strafe);
                    break;
            }
        }

        public override void Disable()
        {
            if (CueSDK.ProtocolDetails != null)
                CueSDK.Reinitialize();
        }

        /// <summary>
        ///     Properly resizes any size bitmap to the keyboard by creating a rectangle whose size is dependent on the bitmap
        ///     size.
        /// </summary>
        /// <param name="bitmap"></param>
        public override void DrawBitmap(Bitmap bitmap)
        {
            var image = ImageUtilities.ResizeImage(bitmap, Width, Height);
            var brush = new ImageBrush
            {
                Image = image
            };

            _keyboard.Brush = brush;
            _keyboard.Update();
        }
    }
}