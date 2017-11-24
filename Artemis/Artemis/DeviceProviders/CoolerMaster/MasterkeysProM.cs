using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using Artemis.DAL;
using Artemis.DeviceProviders.CoolerMaster.Utilities;
using Artemis.DeviceProviders.Logitech.Utilities;
using Artemis.Properties;
using Artemis.Settings;
using Artemis.Utilities;

namespace Artemis.DeviceProviders.CoolerMaster
{
    public class MasterkeysProM : KeyboardProvider
    {
        private GeneralSettings _generalSettings;

        public MasterkeysProM()
        {
            Name = "CM Masterkeys Pro M";
            Slug = "cm-masterkeys-pro-m";

            CantEnableText = "Couldn't connect to your CM Masterkeys Pro M.\n" +
                             "Please check your cables and try updating your CM software.\n\n" +
                             "If needed, you can select a different keyboard in Artemis under settings.";

            Height = 6;
            Width = 19;

            PreviewSettings = new PreviewSettings(new Rect(11, 10, 634, 215), Resources.masterkeys_pro_m);
            _generalSettings = SettingsProvider.Load<GeneralSettings>();
        }

        public override void Disable()
        {
            CmSdk.SetControlDevice(DEVICE_INDEX.DEV_MKeys_M);
            CmSdk.EnableLedControl(false);
            Thread.Sleep(500);
        }

        public override bool CanEnable()
        {
            CmSdk.SetControlDevice(DEVICE_INDEX.DEV_MKeys_M);

            // Doesn't seem reliable but better than nothing I suppose
            return CmSdk.IsDevicePlug();
        }

        public override void Enable()
        {
            CmSdk.SetControlDevice(DEVICE_INDEX.DEV_MKeys_M);
            CmSdk.EnableLedControl(true);
        }

        public override void DrawBitmap(Bitmap bitmap)
        {
            // Resize the bitmap
            using (var b = ImageUtilities.ResizeImage(bitmap, Width, Height))
            {
                // Create an empty matrix
                var matrix = new COLOR_MATRIX {KeyColor = new KEY_COLOR[6, 22]};

                // Map the bytes to the matix
                for (var x = 0; x < Width; x++)
                {
                    for (var y = 0; y < Height; y++)
                    {
                        var c = b.GetPixel(x, y);
                        if (c.R != 0)
                            Console.WriteLine();
                        matrix.KeyColor[y, x] = new KEY_COLOR(c.R, c.G, c.B);
                    }
                }

                // Send the matrix to the keyboard
                CmSdk.SetControlDevice(DEVICE_INDEX.DEV_MKeys_M);
                CmSdk.SetAllLedColor(matrix);
            }
        }

        public override KeyMatch? GetKeyPosition(Keys keyCode)
        {
            switch (_generalSettings.Layout)
            {
                case "Qwerty":
                    return KeyMap.QwertyLayout.FirstOrDefault(k => k.KeyCode == keyCode);
                case "Qwertz":
                    return KeyMap.QwertzLayout.FirstOrDefault(k => k.KeyCode == keyCode);
                default:
                    return KeyMap.AzertyLayout.FirstOrDefault(k => k.KeyCode == keyCode);
            }
        }
    }
}
