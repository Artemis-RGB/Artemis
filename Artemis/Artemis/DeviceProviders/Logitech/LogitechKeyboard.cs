using System.Drawing;
using System.Threading;
using System.Windows;
using Artemis.DeviceProviders.Logitech.Utilities;
using Artemis.Utilities;
using Artemis.Utilities.DataReaders;
using Microsoft.Win32;

namespace Artemis.DeviceProviders.Logitech
{
    public abstract class LogitechKeyboard : KeyboardProvider
    {
        public override bool CanEnable()
        {
            // Check to see if VC++ 2012 x64 is installed.
            if (Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Classes\Installer\Dependencies\{ca67548a-5ebe-413a-b50c-4b9ceb6d66c6}") == null)
            {
                CantEnableText = "Couldn't connect to your Logitech keyboard.\n" +
                                 "The Visual C++ 2012 Redistributable v11.0.61030.0 could not be found, which is required.\n" +
                                 "Please download it by going to the following URL (link also in wiki):\n\n" +
                                 "https://download.microsoft.com/download/1/6/B/16B06F60-3B20-4FF2-B699-5E9B7962F9AE/VSU_4/vcredist_x64.exe";

                return false;
            }

            if (DllManager.RestoreLogitechDll())
                RestoreDll();
            int majorNum = 0, minorNum = 0, buildNum = 0;

            LogitechGSDK.LogiLedInit();
            LogitechGSDK.LogiLedGetSdkVersion(ref majorNum, ref minorNum, ref buildNum);
            LogitechGSDK.LogiLedShutdown();

            // Turn it into one long number...
            var version = int.Parse($"{majorNum}{minorNum}{buildNum}");
            CantEnableText = "Couldn't connect to your Logitech keyboard.\n" +
                             "Please check your cables and updating the Logitech Gaming Software\n" +
                             $"A minimum version of 8.81.15 is required (detected {majorNum}.{minorNum}.{buildNum}).\n\n" +
                             "If the detected version differs from the version LGS is reporting, reinstall LGS or see the FAQ.\n\n" +
                             "If needed, you can select a different keyboard in Artemis under settings.";

            return version >= 88115;
        }

        private void RestoreDll()
        {
            MessageBox.Show(
                "Artemis couldn't enable your Logitech keyboard, because the required files are not in place.\n\n" +
                "This happens when you run The Division and shut down Artemis before shutting down The Division\n" +
                "It can be fixed automatically by clicking OK, but to avoid this message in the future please\n" +
                "shut down The Division before shutting down Artemis.");
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

        public override void DrawBitmap(Bitmap bitmap)
        {
            LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_PERKEY_RGB);
            LogitechGSDK.LogiLedSetLightingFromBitmap(OrionUtilities.BitmapToByteArray(bitmap));
        }
    }
}