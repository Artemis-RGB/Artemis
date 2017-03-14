using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Artemis.DAL;
using Artemis.DeviceProviders.Logitech.Utilities;
using Artemis.Properties;
using Artemis.Settings;

namespace Artemis.DeviceProviders.Logitech
{
    public class G910 : LogitechKeyboard
    {
        private readonly GeneralSettings _generalSettings;

        public G910()
        {
            Name = "Logitech G910 RGB";
            Slug = "logitech-g910";
            CantEnableText = "Couldn't connect to your Logitech G910.\n" +
                             "Please check your cables and updating the Logitech Gaming Software\n" +
                             "A minimum version of 8.81.15 is required.\n\n" +
                             "If needed, you can select a different keyboard in Artemis under settings.";
            Height = 7;
            Width = 22;
            PreviewSettings = new PreviewSettings(new Rect(34, 18, 916, 272), Resources.g910);
            _generalSettings = SettingsProvider.Load<GeneralSettings>();
        }

        public override KeyMatch? GetKeyPosition(Keys keyCode)
        {
            KeyMatch value;
            switch (_generalSettings.Layout)
            {
                case "Qwerty":
                    value = KeyMap.QwertyLayout.FirstOrDefault(k => k.KeyCode == keyCode);
                    break;
                case "Qwertz":
                    value = KeyMap.QwertzLayout.FirstOrDefault(k => k.KeyCode == keyCode);
                    break;
                default:
                    value = KeyMap.AzertyLayout.FirstOrDefault(k => k.KeyCode == keyCode);
                    break;
            }

            // Adjust the distance by 1 on both x and y for the G910
            return new KeyMatch(value.KeyCode, value.X + 1, value.Y + 1);
        }

        /// <summary>
        ///     The G910 also updates the G-logo, G-badge and G-keys
        /// </summary>
        /// <param name="bitmap"></param>
        public override void DrawBitmap(Bitmap bitmap)
        {
            using (var croppedBitmap = new Bitmap(21 * 4, 6 * 4))
            {
                // Deal with non-standard DPI
                croppedBitmap.SetResolution(96, 96);
                // Don't forget that the image is upscaled 4 times
                using (var g = Graphics.FromImage(croppedBitmap))
                {
                    g.DrawImage(bitmap, new Rectangle(0, 0, 84, 24), new Rectangle(4, 4, 84, 24), GraphicsUnit.Pixel);
                }

                base.DrawBitmap(croppedBitmap);
            }

            using (var resized = OrionUtilities.ResizeImage(bitmap, 22, 7))
            {
                // Color the extra keys on the left side of the keyboard
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_LOGO, 0, 1);
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_1, 0, 2);
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_2, 0, 3);
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_3, 0, 4);
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_4, 0, 5);
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_5, 0, 6);

                // Color the extra keys on the top of the keyboard
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_6, 3, 0);
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_7, 4, 0);
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_8, 5, 0);
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_9, 6, 0);

                // Color the G-badge
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_BADGE, 5, 6);
            }
        }

        private void SetLogitechColorFromCoordinates(Bitmap bitmap, KeyboardNames key, int x, int y)
        {
            var color = bitmap.GetPixel(x, y);
            var rPer = (int) Math.Round(color.R / 2.55);
            var gPer = (int) Math.Round(color.G / 2.55);
            var bPer = (int) Math.Round(color.B / 2.55);

            LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(key, rPer, gPer, bPer);
        }
    }
}
