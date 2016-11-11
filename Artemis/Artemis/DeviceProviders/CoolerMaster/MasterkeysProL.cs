using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using Artemis.DeviceProviders.CoolerMaster.Utilities;
using Artemis.DeviceProviders.Logitech.Utilities;
using Artemis.Properties;
using Artemis.Utilities;

namespace Artemis.DeviceProviders.CoolerMaster
{
    public class MasterkeysProL : KeyboardProvider
    {
        private bool _hasControl;

        public MasterkeysProL()
        {
            Name = "CM Masterkeys Pro L";
            Slug = "cm-masterkeys-pro-l";

            CantEnableText = "Couldn't connect to your CM Masterkeys Pro L.\n" +
                             "Please check your cables and try updating your CM software.\n\n" +
                             "If needed, you can select a different keyboard in Artemis under settings.";

            Height = 6;
            Width = 22;

            PreviewSettings = new PreviewSettings(670, 189, new Thickness(-2, -5, 0, 0), Resources.masterkeys_pro_l);
        }

        public override void Disable()
        {
            if (_hasControl)
            {
                CmSdk.EnableLedControl(false);
                Thread.Sleep(500);
            }
            _hasControl = false;
        }

        public override bool CanEnable()
        {
            CmSdk.SetControlDevice(DEVICE_INDEX.DEV_MKeys_L);

            // Doesn't seem reliable but better than nothing I suppose
            return CmSdk.IsDevicePlug();
        }

        public override void Enable()
        {
            CmSdk.SetControlDevice(DEVICE_INDEX.DEV_MKeys_L);

            _hasControl = true;
            CmSdk.EnableLedControl(true);
        }

        public override void DrawBitmap(Bitmap bitmap)
        {
            // Resize the bitmap
            using (var b = ImageUtilities.ResizeImage(bitmap, Width, Height))
            {
                // Create an empty matrix
                var matrix = new COLOR_MATRIX {KeyColor = new KEY_COLOR[Height, Width]};

                // Map the bytes to the matix
                for (var x = 0; x < Width; x++)
                {
                    for (var y = 0; y < Height; y++)
                    {
                        var c = b.GetPixel(x, y);
                        matrix.KeyColor[y, x] = new KEY_COLOR(c.R, c.G, c.B);
                    }
                }

                // Send the matrix to the keyboard
                CmSdk.SetAllLedColor(matrix);
            }
        }

        public override KeyMatch? GetKeyPosition(Keys keyCode)
        {
            return KeyMap.QwertyLayout.FirstOrDefault(k => k.KeyCode == keyCode);
        }
    }
}