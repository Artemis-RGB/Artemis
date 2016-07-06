using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using CUE.NET;
using CUE.NET.Devices.Generic.Enums;
using Ninject.Extensions.Logging;

namespace Artemis.DeviceProviders.Corsair
{
    internal class CorsairMice : DeviceProvider
    {
        public CorsairMice(ILogger logger)
        {
            Logger = logger;
            Type = DeviceType.Mouse;
        }

        public ILogger Logger { get; set; }

        public override bool TryEnable()
        {
            CanUse = CanInitializeSdk();
            if (CanUse && !CueSDK.IsInitialized)
                CueSDK.Initialize();

            Logger.Debug("Attempted to enable Corsair mice. CanUse: {0}", CanUse);
            return CanUse;
        }

        public override void Disable()
        {
            if (CueSDK.IsInitialized)
                CueSDK.Reinitialize();
        }

        public override void UpdateDevice(Bitmap bitmap)
        {
            if (!CanUse || bitmap == null)
                return;
            if (bitmap.Width != bitmap.Height)
                throw new ArgumentException("Bitmap must be a perfect square");

            var leds = CueSDK.MouseSDK.Leds.Count();
            var step = (double) bitmap.Width/leds;
            using (bitmap)
            {
                var ledIndex = 0;
                // Color each LED according to one of the pixels
                foreach (var corsairLed in CueSDK.MouseSDK.Leds)
                {
                    if (ledIndex == 0)
                        corsairLed.Color = bitmap.GetPixel(0, 0);
                    else
                        corsairLed.Color = bitmap.GetPixel((int) ((ledIndex + 1)*step - 1),
                            (int) ((ledIndex + 1)*step - 1));
                    ledIndex++;
                }
            }
            CueSDK.MouseSDK.Update();
        }

        private static bool CanInitializeSdk()
        {
            // This will skip the check-loop if the SDK is initialized
            if (CueSDK.IsInitialized)
                return CueSDK.IsSDKAvailable(CorsairDeviceType.Mouse);

            for (var tries = 0; tries < 9; tries++)
            {
                if (CueSDK.IsSDKAvailable(CorsairDeviceType.Mouse))
                    return true;
                Thread.Sleep(2000);
            }
            return false;
        }
    }
}