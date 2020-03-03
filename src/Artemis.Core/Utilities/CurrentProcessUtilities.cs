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

        public static void RestartSelf()
        {
            var info = new ProcessStartInfo
            {
                Arguments = "/C choice /C Y /N /D Y /T 5 & START /wait taskkill /f /im \"Artemis.UI.exe\" & START \"\" \"" + GetCurrentLocation() + "\"",
                WindowStyle = ProcessWindowStyle.Hidden, 
                CreateNoWindow = true, 
                FileName = "cmd.exe"
            };
            Process.Start(info);
            Execute.OnUIThread(() => Application.Current.Shutdown());
        }
    }
}