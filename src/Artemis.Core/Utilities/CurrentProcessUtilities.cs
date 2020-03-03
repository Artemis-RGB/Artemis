using System.Diagnostics;
using System.Windows;
using Stylet;

namespace Artemis.Core.Utilities
{
    public static class CurrentProcessUtilities
    {
        public static string GetCurrentLocation()
        {
            return Process.GetCurrentProcess().MainModule.FileName;
        }

        public static void Shutdown(int delay, bool restart)
        {
            // Always kill the process after the delay has passed, with all the plugins a graceful shutdown cannot be guaranteed
            var arguments = "-Command \"& {Start-Sleep -s " + delay + "; (Get-Process 'Artemis.UI').kill()}";
            // If restart is required, start the executable again after the process was killed 
            if (restart)
                arguments = "-Command \"& {Start-Sleep -s " + delay + "; (Get-Process 'Artemis.UI').kill(); Start-Process -FilePath '" + GetCurrentLocation() + "'}\"";

            var info = new ProcessStartInfo
            {
                Arguments = arguments,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "PowerShell.exe"
            };
            Process.Start(info);

            // Also attempt a graceful shutdown on the UI thread
            Execute.OnUIThread(() => Application.Current.Shutdown());
        }
    }
}