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

        private const string LogitechPath = @"C:\Program Files\Logitech Gaming Software\SDK\LED\x64\";

        public static bool RestoreLogitechDll()
        {
            if (!File.Exists(LogitechPath + @"\LogitechLed.dll") || !File.Exists(LogitechPath + @"\artemis.txt"))
                return false;

            try
            {
                // Get rid of our own DLL
                File.Delete(LogitechPath + @"\LogitechLed.dll");

                // Restore the backup
                if (File.Exists(LogitechPath + @"\LogitechLed.dll.bak"))
                    File.Copy(LogitechPath + @"\LogitechLed.dll.bak", LogitechPath + @"\LogitechLed.dll");

                File.Delete(LogitechPath + @"\artemis.txt");

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void PlaceLogitechDll()
        {
            if (DllPlaced())
                return;

            // Create directory structure, just in case
            Directory.CreateDirectory(LogitechPath + @"");

            // Backup the existing DLL
            if (File.Exists(LogitechPath + @"\LogitechLed.dll"))
            {
                if (File.Exists(LogitechPath + @"\LogitechLed.dll.bak"))
                    File.Delete(LogitechPath + @"\LogitechLed.dll.bak");
                File.Move(LogitechPath + @"\LogitechLed.dll", LogitechPath + @"\LogitechLed.dll.bak");
            }

            // Copy our own DLL in place
            File.WriteAllBytes(LogitechPath + @"\LogitechLED.dll",
                Resources.LogitechLED);

            // A token to show the file is placed
            File.Create(LogitechPath + @"\artemis.txt");

            // If the user doesn't have a Logitech device, the CLSID will be missing
            // and we should create it ourselves.
            if (!RegistryKeyPlaced())
                PlaceRegistryKey();
        }

        public static bool DllPlaced()
        {
            if (!Directory.Exists(LogitechPath + @""))
                return false;
            if (!RegistryKeyPlaced())
                return false;

            return File.Exists(LogitechPath + @"\artemis.txt");
        }

        private static bool RegistryKeyPlaced()
        {
            var key = Registry
                .LocalMachine.OpenSubKey(
                    @"SOFTWARE\Classes\CLSID\{a6519e67-7632-4375-afdf-caa889744403}\ServerBinary");
            return key != null;
        }

        private static void PlaceRegistryKey()
        {
            var key = Registry
                .LocalMachine.OpenSubKey(
                    @"SOFTWARE\Classes\CLSID\{a6519e67-7632-4375-afdf-caa889744403}\ServerBinary", true);
            key?.SetValue(null, LogitechPath + @"\LogitechLed.dll");
        }

        #endregion
    }
}