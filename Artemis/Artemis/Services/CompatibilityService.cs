using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Win32;
using NLog;

namespace Artemis.Services
{
    public static class CompatibilityService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // Checks to see if RivaTuner Statistics Server is installed and if so places a profile disabling it for Artemis
        public static void CheckRivaTuner()
        {
            // Find the installation path in the registry
            var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Unwinder\RTSS");
            var value = key?.GetValue("InstallPath");
            var installDir = Path.GetDirectoryName(value?.ToString());

            if (installDir == null)
                return;
            var profilePath = Path.Combine(installDir, "ProfileTemplates\\Artemis.exe.cfg");
            if (File.Exists(profilePath))
                return;

            File.WriteAllText(profilePath, "[Hooking]\r\nEnableHooking\t\t= 0");

            // It's kill or be killed...
            var rtssProcess = System.Diagnostics.Process.GetProcessesByName("RTSS").FirstOrDefault();
            var rtssHookProcess = System.Diagnostics.Process.GetProcessesByName("RTSSHooksLoader64").FirstOrDefault();
            rtssProcess?.Kill();
            rtssHookProcess?.Kill();

            // Funnily enough sleeping prevents the RTSS injection so that the process gets killed in time
            Thread.Sleep(1000);

            Logger.Info("Detected that RivaTuner Statistics Server is installed, inserted a profile to prevent crashes.");
        }
    }
}
