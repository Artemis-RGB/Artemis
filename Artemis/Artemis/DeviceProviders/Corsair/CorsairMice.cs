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
            if (CueSDK.ProtocolDetails != null)
                CueSDK.Reinitialize();
        }

        public override void UpdateDevice(Brush brush)
        {
            if (!CanUse || brush == null)
                return;

            var leds = CueSDK.MouseSDK.Leds.Count();
            var rect = new Rect(new Size(leds * 20, leds * 20));
            var img = brush.Dispatcher.Invoke(() =>
            {
                var visual = new DrawingVisual();
                using (var c = visual.RenderOpen())
                    c.DrawRectangle(brush, null, rect);
                return ImageUtilities.DrawinVisualToBitmap(visual, rect);
            });

            var ledIndex = 0;
            // Color each LED according to one of the pixels
            foreach (var corsairLed in CueSDK.MouseSDK.Leds)
            {
                corsairLed.Color = ledIndex == 0
                    ? img.GetPixel(0, 0)
                    : img.GetPixel((ledIndex + 1) * 20 - 1, (ledIndex + 1) * 20 - 1);
                ledIndex++;
            }

            CueSDK.MouseSDK.Update(true);
        }

        private static bool CanInitializeSdk()
        {
            for (var tries = 0; tries < 9; tries++)
            {
                if (CueSDK.IsSDKAvailable(CorsairDeviceType.Keyboard))
                    return true;
                Thread.Sleep(2000);
            }
            return false;
        }
    }
}