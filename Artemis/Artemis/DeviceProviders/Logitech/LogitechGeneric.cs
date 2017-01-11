using System;
using System.Drawing;
using Artemis.DeviceProviders.Logitech.Utilities;
using Ninject.Extensions.Logging;

namespace Artemis.DeviceProviders.Logitech
{
    // TODO: Handle shutdown, maybe implement Disable() afterall?
    public class LogitechGeneric : DeviceProvider
    {
        /// <summary>
        ///     A generic Logitech DeviceProvider. Because the Logitech SDK currently doesn't allow specific
        ///     device targeting (only very broad per-key-RGB and full RGB etc..)
        /// </summary>
        public LogitechGeneric(ILogger logger)
        {
            Logger = logger;
            Type = DeviceType.Generic;
        }

        public ILogger Logger { get; set; }

        public override void UpdateDevice(Bitmap bitmap)
        {
            if (!CanUse || bitmap == null)
                return;

            var col = bitmap.GetPixel(bitmap.Width/2, bitmap.Height/2);
            LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_RGB);
            LogitechGSDK.LogiLedSetLighting((int) (col.R/2.55), (int) (col.G/2.55), (int) (col.B/2.55));
        }

        public override bool TryEnable()
        {
            var majorNum = 0;
            var minorNum = 0;
            var buildNum = 0;

            LogitechGSDK.LogiLedInit();
            LogitechGSDK.LogiLedGetSdkVersion(ref majorNum, ref minorNum, ref buildNum);

            // Turn it into one long number...
            var version = int.Parse($"{majorNum}{minorNum}{buildNum}");
            CanUse = version >= 88115;
            Logger.Debug("Attempted to enable Logitech generic device. CanUse: {0}", CanUse);

            return CanUse;
        }

        public override void Disable()
        {
            throw new NotSupportedException("Can only disable a keyboard");
        }
    }
}