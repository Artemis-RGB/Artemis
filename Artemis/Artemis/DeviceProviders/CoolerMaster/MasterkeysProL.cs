using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using Artemis.DeviceProviders.CoolerMaster.Utilities;
using Artemis.Properties;
using Artemis.Utilities;

namespace Artemis.DeviceProviders.CoolerMaster
{
    public class MasterkeysProL : KeyboardProvider
    {
        public MasterkeysProL()
        {
            Name = "CM Masterkeys Pro L";
            Slug = "cm-masterkeys-pro-l";

            CantEnableText = "Couldn't connect to your CM Masterkeys Pro L.\n" +
                             "Please check your cables and try updating your CM software.\n\n" +
                             "If needed, you can select a different keyboard in Artemis under settings.";

            Height = 6;
            Width = 22;
            // TODO
            PreviewSettings = new PreviewSettings(665, 175, new Thickness(0, -15, 0, 0), Resources.blackwidow);
        }

        public override void Disable()
        {
            CmSdk.EnableLedControl(false);
        }

        public override bool CanEnable()
        {
            return true;
        }

        public override void Enable()
        {
            CmSdk.SetControlDevice(DEVICE_INDEX.DEV_MKeys_L);
            CmSdk.EnableLedControl(true);
        }

        public override void DrawBitmap(Bitmap bitmap)
        {
            using (var b = ImageUtilities.ResizeImage(bitmap, Width, Height))
            {
                var matrix = new COLOR_MATRIX {KeyColor = new KEY_COLOR[Height, Width]};
                for (var x = 0; x < Width; x++)
                {
                    for (var y = 0; y < Height; y++)
                    {
                        var color = b.GetPixel(x, y);
                        matrix.KeyColor[y, x] = new KEY_COLOR(color.R, color.G, color.B);
                    }
                }
                CmSdk.SetAllLedColor(matrix);
            }
        }

        public override KeyMatch? GetKeyPosition(Keys keyCode)
        {
            return null;
        }

        private static byte[,,] BitmapToBytes(Bitmap bitmap)
        {
            BitmapData bitmapData =
                bitmap.LockBits(new Rectangle(new System.Drawing.Point(), bitmap.Size), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            byte[] bitmapBytes;
            var stride = bitmapData.Stride;
            try
            {
                int byteCount = bitmapData.Stride * bitmap.Height;
                bitmapBytes = new byte[byteCount];
                Marshal.Copy(bitmapData.Scan0, bitmapBytes, 0, byteCount);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
            byte[,,] result = new byte[3, bitmap.Width, bitmap.Height];
            for (int k = 0; k < 3; k++)
            {
                for (int i = 0; i < bitmap.Width; i++)
                {
                    for (int j = 0; j < bitmap.Height; j++)
                    {
                        result[k, i, j] = bitmapBytes[j * stride + i * 3 + k];
                    }
                }
            }
            return result;
        }
    }
}