using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Artemis.DeviceProviders.Corsair.Utilities;
using Artemis.Properties;
using Artemis.Utilities;
using CUE.NET;
using CUE.NET.Brushes;
using CUE.NET.Devices.Generic;
using CUE.NET.Devices.Generic.Enums;
using CUE.NET.Exceptions;
using CUE.NET.Helper;
using Ninject.Extensions.Logging;
using Point = System.Drawing.Point;

namespace Artemis.DeviceProviders.Corsair
{
    public class CorsairKeyboard : KeyboardProvider
    {
        private CUE.NET.Devices.Keyboard.CorsairKeyboard _keyboard;
        private ImageBrush _keyboardBrush;

        public CorsairKeyboard(ILogger logger)
        {
            Logger = logger;
            Name = "Corsair RGB Keyboard";
            CantEnableText = "Couldn't connect to your Corsair keyboard.\n" +
                             "Please check your cables and/or drivers (could be outdated) and that Corsair Utility Engine is running.\n" +
                             "In CUE, make sure \"Enable SDK\" is checked under Global Settings.\n\n" +
                             "If needed, you can select a different keyboard in Artemis under settings.";
        }

        public ILogger Logger { get; set; }

        public override bool CanEnable()
        {
            return CueSDK.IsSDKAvailable(CorsairDeviceType.Keyboard);
        }

        /// <summary>
        ///     Enables the SDK and sets updatemode to manual as well as the color of the background to black.
        /// </summary>
        public override void Enable()
        {
            if (!CueSDK.IsInitialized)
                CueSDK.Initialize();

            CueSDK.UpdateMode = UpdateMode.Manual;
            _keyboard = CueSDK.KeyboardSDK;
            switch (_keyboard.DeviceInfo.Model)
            {
                case "K95 RGB":
                    Height = 7;
                    Width = 25;
                    Slug = "corsair-k95-rgb";
                    PreviewSettings = new PreviewSettings(new Thickness(0, -15, 0, 0), Resources.k95);
                    break;
                case "K70 RGB":
                case "K70 RGB RAPIDFIRE":
                case "K70 LUX RGB":
                    Height = 7;
                    Width = 21;
                    Slug = "corsair-k70-rgb";
                    PreviewSettings = new PreviewSettings(new Thickness(0, -25, 0, 0), Resources.k70);
                    break;
                case "K65 RGB":
                case "CGK65 RGB":
                case "K65 LUX RGB":
                case "K65 RGB RAPIDFIRE":
                    Height = 7;
                    Width = 18;
                    Slug = "corsair-k65-rgb";
                    PreviewSettings = new PreviewSettings(new Thickness(0, -30, 0, 0), Resources.k65);
                    break;
                case "STRAFE RGB":
                    Height = 7;
                    Width = 22;
                    Slug = "corsair-strafe-rgb";
                    PreviewSettings = new PreviewSettings(new Thickness(0, -5, 0, 0), Resources.strafe);
                    break;
            }

            Logger.Debug("Corsair SDK reported device as: {0}", _keyboard.DeviceInfo.Model);
            _keyboard.Brush = _keyboardBrush ?? (_keyboardBrush = new ImageBrush());
        }

        public override void Disable()
        {
            try
            {
                if (CueSDK.IsInitialized)
                    CueSDK.Reinitialize();
            }
            catch (WrapperException e)
            {
                // This occurs when releasing the SDK after sleep, ignore it
                if (e.Message != "The previously loaded Keyboard got disconnected.")
                    throw;
            }
        }

        /// <summary>
        ///     Properly resizes any size bitmap to the keyboard by creating a rectangle whose size is dependent on the bitmap
        ///     size.
        /// </summary>
        /// <param name="bitmap"></param>
        public override void DrawBitmap(Bitmap bitmap)
        {
            using (var image = ImageUtilities.ResizeImage(bitmap, Width, Height))
            {
                // For STRAFE, stretch the image on row 2.
                if (_keyboard.DeviceInfo.Model == "STRAFE RGB")
                {
                    using (var strafeBitmap = new Bitmap(22, 8))
                    {
                        using (var g = Graphics.FromImage(strafeBitmap))
                        {
                            g.DrawImage(image, new Point(0, 0));
                            g.DrawImage(image, new Rectangle(0, 3, 22, 7), new Rectangle(0, 2, 22, 7),
                                GraphicsUnit.Pixel);

                            _keyboardBrush.Image = strafeBitmap;
                            _keyboard.Update();
                        }
                    }
                }
                else
                {
                    _keyboardBrush.Image = image;
                    _keyboard.Update();
                }
            }
        }

        public override KeyMatch? GetKeyPosition(Keys keyCode)
        {
            var widthMultiplier = Width / _keyboard.Brush.RenderedRectangle.Width;
            var heightMultiplier = Height / _keyboard.Brush.RenderedRectangle.Height;

            CorsairLed cueLed = null;
            try
            {
                cueLed = _keyboard.Leds.FirstOrDefault(k => k.Id.ToString() == keyCode.ToString()) ??
                         _keyboard.Leds.FirstOrDefault(k => k.Id == KeyMap.FormsKeys[keyCode]);
            }
            catch (Exception)
            {
                // ignored
            }

            if (cueLed == null)
                return null;

            var center = cueLed.LedRectangle.GetCenter();
            return new KeyMatch(keyCode, (int)(center.X * widthMultiplier), (int)(center.Y * heightMultiplier));
        }
    }
}