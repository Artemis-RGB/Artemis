using System.IO;
using Artemis.Properties;
using Microsoft.Win32;

namespace Artemis.Utilities.LogitechDll
{
    internal static class DllManager
    {
        public static void RestoreDll()
        {
            if (!BackupAvailable())
                return;

            // Get rid of our own DLL
            File.Delete(@"C:\Program Files\Logitech Gaming Software\SDK\LED\x64\LogitechLed.dll");
            // Restore the backup
            File.Move(@"C:\Program Files\Logitech Gaming Software\SDK\LED\x64\LogitechLed.dll.bak",
                @"C:\Program Files\Logitech Gaming Software\SDK\LED\x64\LogitechLed.dll");
        }

        public static void PlaceDll()
        {
            if (DllPlaced())
                return;

            // Create directory structure, just in case
            Directory.CreateDirectory(@"C:\Program Files\Logitech Gaming Software\SDK\LED\x64");

            // Remove old backups if they are there
            if (BackupAvailable())
                File.Delete(@"C:\Program Files\Logitech Gaming Software\SDK\LED\x64\LogitechLed.dll.bak");

            // Backup the existing DLL
            if (File.Exists(@"C:\Program Files\Logitech Gaming Software\SDK\LED\x64\LogitechLed.dll"))
                File.Move(@"C:\Program Files\Logitech Gaming Software\SDK\LED\x64\LogitechLed.dll",
                    @"C:\Program Files\Logitech Gaming Software\SDK\LED\x64\LogitechLed.dll.bak");

            // Copy our own DLL in place
            File.WriteAllBytes(@"C:\Program Files\Logitech Gaming Software\SDK\LED\x64\LogitechLED.dll",
                Resources.LogitechLED);

            // If the user doesn't have a Logitech device, the CLSID will be missing
            // and we should create it ourselves.
            if (!RegistryKeyPlaced())
                PlaceRegistryKey();
        }

        public static bool DllPlaced()
        {
            if (!Directory.Exists(@"C:\Program Files\Logitech Gaming Software\SDK\LED\x64"))
                return false;
            if (!RegistryKeyPlaced())
                return false;

            return File.Exists(@"C:\Program Files\Logitech Gaming Software\SDK\LED\x64\LogitechLed.dll");
        }

        private static bool BackupAvailable()
        {
            return File.Exists(@"C:\Program Files\Logitech Gaming Software\SDK\LED\x64\LogitechLed.dll.bak");
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
            key?.SetValue(null, @"C:\Program Files\Logitech Gaming Software\SDK\LED\x64\LogitechLed.dll");
        }
    }
}