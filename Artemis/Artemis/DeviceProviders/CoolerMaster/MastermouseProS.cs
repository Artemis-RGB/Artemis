using System;
using System.Drawing;
using Artemis.DeviceProviders.CoolerMaster.Utilities;
using Ninject.Extensions.Logging;

namespace Artemis.DeviceProviders.CoolerMaster
{
    public class MastermouseProS : DeviceProvider
    {
        public MastermouseProS(ILogger logger)
        {
            Logger = logger;
            Type = DeviceType.Mouse;
        }

        public ILogger Logger { get; }

        public override void UpdateDevice(Bitmap bitmap)
        {
            // Create an empty matrix
            var matrix = new COLOR_MATRIX {KeyColor = new KEY_COLOR[6, 22]};

            // Get colors from the bitmap's center X and on 1/3rd and 2/3rd Y
            var x = bitmap.Width / 2;
            var y = bitmap.Width / 3;
            var led1Color = bitmap.GetPixel(x, y);
            var led2Color = bitmap.GetPixel(x, y * 2);
            matrix.KeyColor[0, 0] = new KEY_COLOR(led1Color.R, led1Color.G, led1Color.B);
            matrix.KeyColor[0, 1] = new KEY_COLOR(led2Color.R, led2Color.G, led2Color.B);

            // Send the matrix to the mouse
            CmSdk.SetControlDevice(DEVICE_INDEX.DEV_MMouse_S);
            CmSdk.SetAllLedColor(matrix);
        }

        public override bool TryEnable()
        {
            CmSdk.SetControlDevice(DEVICE_INDEX.DEV_MMouse_S);

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
            
            Logger.Debug("Attempted to enable Mastermouse Pro S. CanUse: {0}", CanUse);
            return CanUse;
        }

        public override void Disable()
        {
            throw new NotSupportedException("Can only disable a keyboard");
        }
    }
}
