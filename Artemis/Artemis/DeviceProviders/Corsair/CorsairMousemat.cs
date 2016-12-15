using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using CUE.NET;
using CUE.NET.Devices.Generic.Enums;
using Ninject.Extensions.Logging;

namespace Artemis.DeviceProviders.Corsair
{
    internal class CorsairMousemat : DeviceProvider
    {
        public CorsairMousemat(ILogger logger)
        {
            Logger = logger;
            Type = DeviceType.Mousemat;
        }

        public ILogger Logger { get; set; }

        public override bool TryEnable()
        {
            CanUse = CanInitializeSdk();
            if (CanUse && !CueSDK.IsInitialized)
                CueSDK.Initialize(true);

            Logger.Debug("Attempted to enable Corsair mousemat. CanUse: {0}", CanUse);

            if (CanUse)
                CueSDK.UpdateMode = UpdateMode.Manual;

            return CanUse;
        }

        public override void Disable()
        {
            throw new NotImplementedException("Can only disable a keyboard");
        }

        public override void UpdateDevice(Bitmap bitmap)
        {
            if (!CanUse || (bitmap == null))
                return;
            if (bitmap.Width != bitmap.Height)
                throw new ArgumentException("Bitmap must be a perfect square");

            var yStep = (double) bitmap.Width/4;
            var xStep = (double) bitmap.Width/6;

            // This approach will break if any mousemats with different LED amounts are released, for now it will do.
            var ledIndex = 0;
            // Color each LED according to one of the pixels
            foreach (var corsairLed in CueSDK.MousematSDK.Leds.OrderBy(l => l.ToString()))
            {
                Color col;
                // Left side
                if (ledIndex < 5)
                {
                    col = ledIndex == 0
                        ? bitmap.GetPixel(0, (int) (ledIndex*yStep))
                        : bitmap.GetPixel(0, (int) (ledIndex*yStep) - 1);
                }
                // Bottom
                else if (ledIndex < 10)
                {
                    // Start at index 1 because the corner belongs to the left side
                    var zoneIndex = ledIndex - 4;
                    col = bitmap.GetPixel((int) (zoneIndex*xStep), bitmap.Height - 1);
                }
                // Right side
                else
                {
                    var zoneIndex = ledIndex - 10;
                    col = zoneIndex == 4
                        ? bitmap.GetPixel(bitmap.Height - 1, bitmap.Height - (int) (zoneIndex*yStep))
                        : bitmap.GetPixel(bitmap.Height - 1, bitmap.Height - 1 - (int) (zoneIndex*yStep));
                }

                corsairLed.Color = col;
                ledIndex++;
            }
            CueSDK.MousematSDK.Update();
        }

        private static bool CanInitializeSdk()
        {
            // This will skip the check-loop if the SDK is initialized
            if (CueSDK.IsInitialized)
                return CueSDK.IsSDKAvailable(CorsairDeviceType.Mousemat);

            for (var tries = 0; tries < 9; tries++)
            {
                if (CueSDK.IsSDKAvailable(CorsairDeviceType.Mousemat))
                    return true;
                Thread.Sleep(2000);
            }
            return false;
        }
    }
}