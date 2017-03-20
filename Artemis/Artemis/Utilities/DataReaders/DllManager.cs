using System;
using System.IO;
using Artemis.Properties;
using Microsoft.Win32;
using NLog;

namespace Artemis.Utilities.DataReaders
{
    internal static class DllManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Razer

        public static void PlaceRazerDll(string path)
        {
            try
            {
                File.WriteAllBytes(path + @"\RzChromaSDK64.dll", Resources.RzChromaSDK64);
                Logger.Debug("Successfully placed Razer DLL in {0}", path);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Couldn't place Razer DLL in {0}", path);
            }
        }

        #endregion

        #region Logitech

        private static readonly string LogitechPath = @"C:\Program Files\Logitech Gaming Software\SDK\LED\x64\";
        private static readonly string DllPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Artemis\dll";

        public static void PlaceLogitechDll()
        {
            try
            {
                // Change the registry key to point to the fake DLL
                var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\CLSID\{a6519e67-7632-4375-afdf-caa889744403}\ServerBinary", true);
                key?.SetValue(null, DllPath + @"\LogitechLed.dll");

                // Make sure the fake DLL is in place
                if (!Directory.Exists(DllPath))
                    Directory.CreateDirectory(DllPath);
                if (!File.Exists(DllPath + @"\LogitechLed.dll"))
                    File.WriteAllBytes(DllPath + @"\LogitechLED.dll", Resources.LogitechLED);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to place Logitech DLL");
            }
        }

        public static void RestoreLogitechDll()
        {
            // Change the registry key to point to the real DLL
            var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\CLSID\{a6519e67-7632-4375-afdf-caa889744403}\ServerBinary", true);
            key?.SetValue(null, LogitechPath + @"\LogitechLed.dll");
        }

        #endregion
    }
}
