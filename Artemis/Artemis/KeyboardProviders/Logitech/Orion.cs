using System.Drawing;
using System.Threading;
using Artemis.KeyboardProviders.Logitech.Utilities;

namespace Artemis.KeyboardProviders.Logitech
{
    internal class Orion : KeyboardProvider
    {
        public Orion()
        {
            Name = "Logitech G910 Orion Spark RGB";
        }

        public override void Enable()
        {
            // Initialize the SDK
            LogitechGSDK.LogiLedInit();
            Thread.Sleep(200);
            LogitechGSDK.LogiLedSaveCurrentLighting();
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