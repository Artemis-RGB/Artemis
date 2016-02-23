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
            CantEnableText = "Couldn't connect to your Logitech G910.\n " +
                             "Please check your cables and updating the Logitech Gaming Software.\n\n " +
                             "If needed, you can select a different keyboard in Artemis under settings.";
            Height = 6;
            Width = 21;
            KeyboardRegions = new List<KeyboardRegion> {new KeyboardRegion("TopRow", new Point(0, 0), new Point(0, 16))};
        }

        public override bool CanEnable()
        {
            // TODO
            return true;
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