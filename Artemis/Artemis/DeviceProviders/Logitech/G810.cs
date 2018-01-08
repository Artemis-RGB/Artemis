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
    public class G810 : LogitechKeyboard
    {
        private GeneralSettings _generalSettings;

        public G810()
        {
            Name = "Logitech G810 RGB";
            Slug = "logitech-g810";
            CantEnableText = "Couldn't connect to your Logitech G810.\n" +
                             "Please check your cables and updating the Logitech Gaming Software\n" +
                             "A minimum version of 8.81.15 is required.\n\n" +
                             "If needed, you can select a different keyboard in Artemis under settings.";
            Height = 7;
            Width = 21;
            PreviewSettings = new PreviewSettings(new Rect(19, 36, 1010, 304), Resources.g810);
            _generalSettings = SettingsProvider.Load<GeneralSettings>();
        }

        /// <summary>
        ///     The 8910 also updates the G-logo and G-badge
        /// </summary>
        /// <param name="bitmap"></param>
        public override void DrawBitmap(Bitmap bitmap)
        {
            // Base all actions on a downscaled bitmap since the bitmap is received upscaled
            using (var resized = OrionUtilities.ResizeImage(bitmap, Width, Height))
            {
                // Deal with non-standard DPI
                resized.SetResolution(96, 96);

                // LogiLedSetLightingFromBitmap doesn't include the special keys so cut the supported keys out of
                // the received bitmap and send them to the SDK as a whole
                using (var croppedBitmap = new Bitmap(Width - 1, Height - 1))
                {
                    croppedBitmap.SetResolution(96, 96);
                    using (var g = Graphics.FromImage(croppedBitmap))
                    {
                        g.DrawImage(resized, new Rectangle(0, 0, Width - 1, Height - 1), new Rectangle(1, 1, Width - 1, Height - 1), GraphicsUnit.Pixel);
                    }

                    LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_PERKEY_RGB);
                    LogitechGSDK.LogiLedSetLightingFromBitmap(OrionUtilities.BitmapToByteArray(croppedBitmap));
                }

                // Color G-logo, lets also try some other values to see what happens
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_LOGO, 0, 0);
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_BADGE, 0, 0);
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_1, 0, 0);
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_2, 0, 0);
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_3, 0, 0);
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_4, 0, 0);
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_5, 0, 0);
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_6, 0, 0);
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_7, 0, 0);
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_8, 0, 0);
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_9, 0, 0);
            }
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

            // Adjust the distance by 1 on y for the G810
            return new KeyMatch(value.KeyCode, value.X, value.Y + 1);
        }
    }
}