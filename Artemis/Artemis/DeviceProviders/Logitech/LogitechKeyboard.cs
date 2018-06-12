using System;
using System.Drawing;
using System.Threading;
using Artemis.Utilities.DataReaders;
using LedCSharp;

namespace Artemis.DeviceProviders.Logitech
{
    public abstract class LogitechKeyboard : KeyboardProvider
    {
        public override bool CanEnable()
        {
            // Just to be sure, restore the Logitech DLL registry key
            DllManager.RestoreLogitechDll();

            int majorNum = 0, minorNum = 0, buildNum = 0;

            LogitechGSDK.LogiLedInit();
            LogitechGSDK.LogiLedGetSdkVersion(ref majorNum, ref minorNum, ref buildNum);
            LogitechGSDK.LogiLedShutdown();

            CantEnableText = "Couldn't connect to your Logitech keyboard.\n" +
                             "Please check your cables and updating the Logitech Gaming Software\n" +
                             $"A minimum version of 8.81.15 is required (detected {majorNum}.{minorNum}.{buildNum}).\n\n" +
                             "If the detected version differs from the version LGS is reporting, reinstall LGS or see the FAQ.\n\n" +
                             "If needed, you can select a different keyboard in Artemis under settings.";

            return majorNum >= 9;
        }

        public override void Enable()
        {
            // Initialize the SDK
            LogitechGSDK.LogiLedInit();
            Thread.Sleep(200);

            LogitechGSDK.LogiLedSaveCurrentLighting();
            LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_PERKEY_RGB);
        }

        public override void Disable()
        {
            // Shutdown the SDK
            LogitechGSDK.LogiLedRestoreLighting();
            LogitechGSDK.LogiLedShutdown();
        }

        protected void SetLogitechColorFromCoordinates(Bitmap bitmap, keyboardNames key, int x, int y)
        {
            var color = bitmap.GetPixel(x, y);
            var rPer = (int) Math.Round(color.R / 2.55);
            var gPer = (int) Math.Round(color.G / 2.55);
            var bPer = (int) Math.Round(color.B / 2.55);

            LogitechGSDK.LogiLedSetLightingForKeyWithKeyName(key, rPer, gPer, bPer);
        }
    }
}