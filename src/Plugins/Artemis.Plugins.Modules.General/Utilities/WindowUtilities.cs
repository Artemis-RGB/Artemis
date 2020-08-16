using System;
using System.Runtime.InteropServices;

namespace Artemis.Plugins.Modules.General.Utilities
{
    public static class WindowUtilities
    {
        public static int GetActiveProcessId()
        {
            var hWnd = GetForegroundWindow(); // Get foreground window handle
            GetWindowThreadProcessId(hWnd, out var processId);
            return (int) processId;
        }


        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    }
}