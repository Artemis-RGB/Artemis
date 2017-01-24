using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using CUE.NET;
using CUE.NET.Devices.Generic.Enums;
using Ninject.Extensions.Logging;

namespace Artemis.DeviceProviders.Corsair
{
    public class CorsairHeadset : DeviceProvider
    {
        public CorsairHeadset(ILogger logger)
        {
            Logger = logger;
            Type = DeviceType.Headset;
        }

        public ILogger Logger { get; set; }

        public override bool TryEnable()
        {
            CanUse = CanInitializeSdk();
            if (CanUse && !CueSDK.IsInitialized)
                CueSDK.Initialize();

            Logger.Debug("Attempted to enable Corsair headset. CanUse: {0}", CanUse);

            if (CanUse)
                CueSDK.HeadsetSDK.UpdateMode = UpdateMode.Manual;

            return CanUse;
        }

        public override void Disable()
        {
            throw new NotSupportedException("Can only disable a keyboard");
        }

        public override void UpdateDevice(Bitmap bitmap)
        {
            if (!CanUse || bitmap == null)
                return;
            if (bitmap.Width != bitmap.Height)
                throw new ArgumentException("Bitmap must be a perfect square");

            var leds = CueSDK.HeadsetSDK.Leds.Count();
            var step = (double) bitmap.Width/leds;

            var ledIndex = 0;
            // Color each LED according to one of the pixels
            foreach (var corsairLed in CueSDK.HeadsetSDK.Leds)
            {
                var col = ledIndex == 0
                    ? bitmap.GetPixel(0, 0)
                    : bitmap.GetPixel((int) ((ledIndex + 1)*step - 1), (int) ((ledIndex + 1)*step - 1));

                corsairLed.Color = col;
                ledIndex++;
            }

            CueSDK.HeadsetSDK.Update();
        }

        private static bool CanInitializeSdk()
        {
            // This will skip the check-loop if the SDK is initialized
            if (CueSDK.IsInitialized)
                return CueSDK.IsSDKAvailable(CorsairDeviceType.Headset);

            for (var tries = 0; tries < 9; tries++)
            {
                if (CueSDK.IsSDKAvailable(CorsairDeviceType.Headset))
                    return true;
                Thread.Sleep(2000);
            }
            return false;
        }
    }
}