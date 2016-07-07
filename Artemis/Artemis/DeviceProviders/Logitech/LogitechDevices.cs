using System;
using System.Drawing;
using Artemis.DeviceProviders.Logitech.Utilities;
using Ninject.Extensions.Logging;

namespace Artemis.DeviceProviders.Logitech
{
    public class LogitechDevices : DeviceProvider
    {
        /// <summary>
        ///     A generic Logitech DeviceProvider. Because the Logitech SDK currently doesn't allow specific
        ///     device targeting (only very broad per-key-RGB and full RGB etc..) all non-keyboards share this provider
        /// </summary>
        /// TODO: Generic device type. For now, this provider just pretends to be a headset
        public LogitechDevices(ILogger logger)
        {
            Logger = logger;
            Type = DeviceType.Headset;
        }

        public ILogger Logger { get; set; }

        public override void UpdateDevice(Bitmap bitmap)
        {
            if (!CanUse || bitmap == null)
                return;
            if (bitmap.Width != bitmap.Height)
                throw new ArgumentException("Bitmap must be a perfect square");

            using (bitmap)
            {
                var col = bitmap.GetPixel(bitmap.Width/2, bitmap.Height/2);
                LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_RGB);
                LogitechGSDK.LogiLedSetLighting((int) (col.R/2.55), (int) (col.G/2.55), (int) (col.B/2.55));
            }
        }

        public override bool TryEnable()
        {
            var majorNum = 0;
            var minorNum = 0;
            var buildNum = 0;

            LogitechGSDK.LogiLedInit();
            LogitechGSDK.LogiLedGetSdkVersion(ref majorNum, ref minorNum, ref buildNum);
            LogitechGSDK.LogiLedShutdown();

            // Turn it into one long number...
            var version = int.Parse($"{majorNum}{minorNum}{buildNum}");
            CanUse = version >= 88115;
            Logger.Debug("Attempted to enable Logitech generic device. CanUse: {0}", CanUse);

            return CanUse;
        }

        public override void Disable()
        {
            throw new NotImplementedException("Can only disable a keyboard");
        }
    }
}