using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using Artemis.KeyboardProviders.Logitech.Utilities;

namespace Artemis.KeyboardProviders.Logitech
{
    internal class Orion : KeyboardProvider
    {
        public Orion()
        {
            Name = "Logitech G910 RGB";
            CantEnableText = "Couldn't connect to your Logitech G910.\n" +
                             "Please check your cables and updating the Logitech Gaming Software\n" +
                             "A minimum version of 8.81.15 is required).\n\n" +
                             "If needed, you can select a different keyboard in Artemis under settings.";
            Height = 6;
            Width = 21;
            KeyboardRegions = new List<KeyboardRegion>
            {
                new KeyboardRegion("TopRow", new Point(0, 0), new Point(0, 16)),
                new KeyboardRegion("NumPad", new Point(0, 17), new Point(0, 25))
            };
        }

        public override bool CanEnable()
        {
            int majorNum = 0, minorNum = 0, buildNum = 0;

            LogitechGSDK.LogiLedInit();
            LogitechGSDK.LogiLedGetSdkVersion(ref majorNum, ref minorNum, ref buildNum);
            LogitechGSDK.LogiLedShutdown();

            // Turn it into one long number...
            var version = int.Parse($"{majorNum}{minorNum}{buildNum}");
            return version >= 88115;
        }

        public override void Enable()
        {
            // Initialize the SDK
            LogitechGSDK.LogiLedInit();
            Thread.Sleep(200);

            LogitechGSDK.LogiLedSaveCurrentLighting();
            LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_PERKEY_RGB);

            // Disable keys we can't color
            LogitechGSDK.LogiLedSetLighting(0, 0, 0);
        }

        public override void Disable()
        {
            // Shutdown the SDK
            LogitechGSDK.LogiLedRestoreLighting();
            LogitechGSDK.LogiLedShutdown();
        }

        public override void DrawBitmap(Bitmap bitmap)
        {
            LogitechGSDK.LogiLedSetLightingFromBitmap(OrionUtilities.BitmapToByteArray(bitmap));
        }
    }
}