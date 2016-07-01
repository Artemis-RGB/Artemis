using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Artemis.Utilities;
using CUE.NET;
using CUE.NET.Devices.Generic.Enums;
using Ninject.Extensions.Logging;

namespace Artemis.DeviceProviders.Corsair
{
    internal class CorsairHeadsets : DeviceProvider
    {
        public CorsairHeadsets(ILogger logger)
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
            return CanUse;
        }

        public override void Disable()
        {
            if (CueSDK.IsInitialized)
                CueSDK.Reinitialize();
        }

        public override void UpdateDevice(Brush brush)
        {
            if (!CanUse || brush == null)
                return;

            var leds = CueSDK.HeadsetSDK.Leds.Count();
            var rect = new Rect(new Size(leds*20, leds*20));

            var visual = new DrawingVisual();
            using (var c = visual.RenderOpen())
                c.DrawRectangle(brush, null, rect);

            using (var img = ImageUtilities.DrawinVisualToBitmap(visual, rect))
            {
                var ledIndex = 0;
                // Color each LED according to one of the pixels
                foreach (var corsairLed in CueSDK.HeadsetSDK.Leds)
                {
                    corsairLed.Color = ledIndex == 0
                        ? img.GetPixel(0, 0)
                        : img.GetPixel((ledIndex + 1)*20 - 1, (ledIndex + 1)*20 - 1);
                    ledIndex++;
                }
            }
            // Flush is required for headset to work reliably on CUE2 for some reason
            CueSDK.HeadsetSDK.Update(true);
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