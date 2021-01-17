using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Artemis.UI.Utilities
{
    public static class WindowUtilities
    {
        public static int GetActiveProcessId()
        {
            // Get foreground window handle
            IntPtr hWnd = GetForegroundWindow();

            GetWindowThreadProcessId(hWnd, out uint processId);
            return (int) processId;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    }
}