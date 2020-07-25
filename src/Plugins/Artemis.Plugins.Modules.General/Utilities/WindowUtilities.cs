using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

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

        public static string GetProcessFilename(Process p)
        {
            var capacity = 2000;
            var builder = new StringBuilder(capacity);
            var ptr = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, p.Id);
            if (!QueryFullProcessImageName(ptr, 0, builder, ref capacity)) return string.Empty;

            return builder.ToString();
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] int dwFlags, [Out] StringBuilder lpExeName, ref int lpdwSize);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);


        [Flags]
        private enum ProcessAccessFlags : uint
        {
            QueryLimitedInformation = 0x00001000
        }
    }
}