using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Artemis.Utilities
{
    public static class ActiveWindowHelper
    {
        #region DLL-Imports

        private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        #endregion

        #region Constants

        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;

        private const int MAX_TITLE_LENGTH = 256;

        #endregion

        #region Properties & Fields

        // DarthAffe 17.12.2016: We need to keep a reference to this or it might get collected by the garbage collector and cause some random crashes afterwards.
        private static WinEventDelegate _activeWindowChangedDelegate;
        private static IntPtr _winEventHook;

        public static string ActiveWindowProcessName { get; private set; }
        public static string ActiveWindowWindowTitle { get; private set; }

        #endregion

        #region Methods

        private static void ActiveWindowChanged(IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            ActiveWindowProcessName = GetActiveWindowProcessName(hwnd) ?? string.Empty;
            ActiveWindowWindowTitle = GetActiveWindowTitle(hwnd) ?? string.Empty;
        }

        private static string GetActiveWindowProcessName(IntPtr hwnd)
        {
            try
            {
                uint pid;
                GetWindowThreadProcessId(hwnd, out pid);
                return System.Diagnostics.Process.GetProcessById((int)pid).ProcessName;
            }
            catch
            {
                return null;
            }
        }

        private static string GetActiveWindowTitle(IntPtr hwnd)
        {
            try
            {
                StringBuilder buffer = new StringBuilder(MAX_TITLE_LENGTH);
                return GetWindowText(hwnd, buffer, MAX_TITLE_LENGTH) > 0 ? buffer.ToString() : null;
            }
            catch
            {
                return null;
            }
        }

        public static void Initialize()
        {
            try
            {
                if (_winEventHook != IntPtr.Zero) return;

                _activeWindowChangedDelegate = ActiveWindowChanged;
                _winEventHook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, _activeWindowChangedDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
            }
            catch { /* catch'em all - I don't want it to crash here */ }
        }

        public static void Dispose()
        {
            try
            {
                if (_winEventHook == IntPtr.Zero) return;

                UnhookWinEvent(_winEventHook);
                _activeWindowChangedDelegate = null;
                _winEventHook = IntPtr.Zero;
            }
            catch { /* catch'em all - I don't want it to crash here */ }
        }

        #endregion
    }
}
