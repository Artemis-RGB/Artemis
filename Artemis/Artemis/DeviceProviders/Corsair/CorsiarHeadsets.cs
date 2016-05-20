using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Artemis.Utilities;
using CUE.NET;
using CUE.NET.Devices.Generic.Enums;
using CUE.NET.Exceptions;

namespace Artemis.DeviceProviders.Corsair
{
    internal class CorsairHeadsets : DeviceProvider
    {
        public CorsairHeadsets()
        {
            Type = DeviceType.Headset;
        }

        public override bool TryEnable()
        {
            CanUse = CanInitializeSdk();
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

            var leds = CueSDK.HeadsetSDK.Leds.Count();
            var rect = new Rect(new Size(leds * 5, leds * 5));
            var img = brush.Dispatcher.Invoke(() =>
            {
                var visual = new DrawingVisual();
                using (var c = visual.RenderOpen())
                    c.DrawRectangle(brush, null, rect);
                return ImageUtilities.DrawinVisualToBitmap(visual, rect);
            });

            var ledIndex = 0;
            // Color each LED according to one of the pixels
            foreach (var corsairLed in CueSDK.HeadsetSDK.Leds)
            {
                corsairLed.Color = img.GetPixel(ledIndex * 5, ledIndex * 5);
                ledIndex++;
            }
            CueSDK.HeadsetSDK.Update(true);
        }

        private static bool CanInitializeSdk()
        {
            // Try for about 10 seconds, in case CUE isn't started yet
            var tries = 0;
            while (tries < 9)
            {
                try
                {
                    if (CueSDK.ProtocolDetails == null)
                        CueSDK.Initialize();
                    else
                        return true;
                }
                catch (CUEException e)
                {
                    if (e.Error != CorsairError.ServerNotFound)
                        return true;

                    tries++;
                    Thread.Sleep(1000);
                    continue;
                }
                catch (WrapperException)
                {
                    CueSDK.Reinitialize();
                    return true;
                }

                return true;
            }

            return false;
        }
    }
}