using System;
using System.Drawing;
using Artemis.DeviceProviders.CoolerMaster.Utilities;
using Ninject.Extensions.Logging;

namespace Artemis.DeviceProviders.CoolerMaster
{
    public class MastermouseProL : DeviceProvider
    {
        public MastermouseProL(ILogger logger)
        {
            Logger = logger;
            Type = DeviceType.Mouse;
        }

        public ILogger Logger { get; }

        public override void UpdateDevice(Bitmap bitmap)
        {
            // Create an empty matrix
            var matrix = new COLOR_MATRIX { KeyColor = new KEY_COLOR[6, 22] };

            // Get colors from the bitmap's center X and on 2/5th, 3/5th and 4/5th Y
            var x = bitmap.Width / 2;
            var y = bitmap.Width / 5;
            var led1Color = bitmap.GetPixel(x, y);
            var led2Color = bitmap.GetPixel(x, y * 2);
            var led3Color = bitmap.GetPixel(x, y * 3);
            var led4Color = bitmap.GetPixel(x, y * 4);
            matrix.KeyColor[0, 0] = new KEY_COLOR(led1Color.R, led1Color.G, led1Color.B);
            matrix.KeyColor[0, 1] = new KEY_COLOR(led2Color.R, led2Color.G, led2Color.B);
            matrix.KeyColor[0, 2] = new KEY_COLOR(led3Color.R, led3Color.G, led3Color.B);
            matrix.KeyColor[0, 3] = new KEY_COLOR(led4Color.R, led4Color.G, led4Color.B);

            // Send the matrix to the mouse
            CmSdk.SetControlDevice(DEVICE_INDEX.DEV_MMouse_L);
            CmSdk.SetAllLedColor(matrix);
        }

        public override bool TryEnable()
        {
            CmSdk.SetControlDevice(DEVICE_INDEX.DEV_MMouse_L);

            // Doesn't seem reliable but better than nothing I suppose
            try
            {
                CanUse = CmSdk.IsDevicePlug();
                if (CanUse)
                    CmSdk.EnableLedControl(true);
            }
            catch (Exception)
            {
                CanUse = false;
            }

            Logger.Debug("Attempted to enable Mastermouse Pro L. CanUse: {0}", CanUse);
            return CanUse;
        }

        public override void Disable()
        {
            throw new NotSupportedException("Can only disable a keyboard");
        }
    }
}
