using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Artemis.Utilities.ActiveWindowDetection
{
    public class EventActiveWindowDetector : IActiveWindowDetector
    {
        #region DLL-Imports

        private delegate void WinEventDelegate(
            IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread,
            uint dwmsEventTime);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
                                                     WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        #endregion

        #region Constants

        private const uint WINEVENT_OUTOFCONTEXT = 0x0000;
        private const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
        private const uint EVENT_OBJECT_NAMECHANGE = 0x800C;
        private const uint EVENT_SYSTEM_MINIMIZEEND = 0x0017;

        private const int MAX_TITLE_LENGTH = 256;

        #endregion

        #region Properties & Fields

        // DarthAffe 17.12.2016: We need to keep a reference to this or it might get collected by the garbage collector and cause some random crashes afterwards.
        private WinEventDelegate _activeWindowChangedDelegate;
        private IntPtr _activeWindowEventHook;

        private WinEventDelegate _windowTitleChangedDelegate;
        private IntPtr _windowTitleEventHook;

        private WinEventDelegate _windowMinimizedChangedDelegate;
        private IntPtr _windowMinimizedEventHook;

        private IntPtr _activeWindow;

        public string ActiveWindowProcessName { get; private set; } = string.Empty;
        public string ActiveWindowWindowTitle { get; private set; } = string.Empty;

        #endregion

        #region Methods

        private void ActiveWindowChanged(IntPtr hWinEventHook, uint eventType,
                                                IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            UpdateForWindow(hwnd);
        }

        private void WindowTitleChanged(IntPtr hWinEventHook, uint eventType,
                                               IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (_activeWindow == hwnd)
                UpdateForWindow(hwnd);
        }

        private void WindowMinimizedChanged(IntPtr hWinEventHook, uint eventType,
                                                   IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            // DarthAffe 19.12.2016: We expect currently un-minimized windows to be active.
            // DarthAffe 19.12.2016: The result of the API-function GetActiveWindow at this moment is 'idle' so we can't use this to validate this estimation.
            UpdateForWindow(hwnd);
        }

        private void UpdateForWindow(IntPtr hwnd)
        {
            _activeWindow = hwnd;

            ActiveWindowProcessName = GetActiveWindowProcessName(hwnd) ?? string.Empty;
            ActiveWindowWindowTitle = GetActiveWindowTitle(hwnd) ?? string.Empty;
        }

        private string GetActiveWindowProcessName(IntPtr hwnd)
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

        private string GetActiveWindowTitle(IntPtr hwnd)
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

        public void Initialize()
        {
            try
            {
                if (_activeWindowEventHook == IntPtr.Zero)
                {
                    _activeWindowChangedDelegate = ActiveWindowChanged;
                    _activeWindowEventHook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND,
                                                             IntPtr.Zero, _activeWindowChangedDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
                }

                if (_windowTitleEventHook == IntPtr.Zero)
                {
                    _windowTitleChangedDelegate = WindowTitleChanged;
                    _windowTitleEventHook = SetWinEventHook(EVENT_OBJECT_NAMECHANGE, EVENT_OBJECT_NAMECHANGE,
                                                            IntPtr.Zero, _windowTitleChangedDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
                }

                if (_windowMinimizedEventHook == IntPtr.Zero)
                {
                    _windowMinimizedChangedDelegate = WindowMinimizedChanged;
                    _windowMinimizedEventHook = SetWinEventHook(EVENT_SYSTEM_MINIMIZEEND, EVENT_SYSTEM_MINIMIZEEND,
                                                                IntPtr.Zero, _windowMinimizedChangedDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
                }
            }
            catch
            {
                /* catch'em all - I don't want it to crash here */
            }
        }

        public void Dispose()
        {
            try
            {
                if (_activeWindowEventHook != IntPtr.Zero)
                {
                    UnhookWinEvent(_activeWindowEventHook);
                    _activeWindowChangedDelegate = null;
                    _activeWindowEventHook = IntPtr.Zero;
                }

                if (_windowTitleEventHook != IntPtr.Zero)
                {
                    UnhookWinEvent(_windowTitleEventHook);
                    _windowTitleChangedDelegate = null;
                    _windowTitleEventHook = IntPtr.Zero;
                }

                if (_windowMinimizedEventHook != IntPtr.Zero)
                {
                    UnhookWinEvent(_windowMinimizedEventHook);
                    _windowMinimizedChangedDelegate = null;
                    _windowMinimizedEventHook = IntPtr.Zero;
                }
            }
            catch
            {
                /* catch'em all - I don't want it to crash here */
            }
        }

        #endregion
    }
}
