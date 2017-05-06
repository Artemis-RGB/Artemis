using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;

namespace Artemis.Utilities.ActiveWindowDetection
{
    public class TimerActiveWindowDetector : IActiveWindowDetector
    {
        #region DLL-Imports

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        #endregion

        #region Constants

        private const int TIMER_INTERVAL = 1000;
        private const int MAX_TITLE_LENGTH = 256;

        #endregion

        #region Properties & Fields

        private Timer _timer;

        public string ActiveWindowProcessName { get; private set; }
        public string ActiveWindowWindowTitle { get; private set; }

        #endregion

        #region Methods

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            IntPtr activeWindow = GetForegroundWindow();
            ActiveWindowProcessName = GetActiveWindowProcessName(activeWindow);
            ActiveWindowWindowTitle = GetActiveWindowTitle(activeWindow);
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
            _timer = new Timer(TIMER_INTERVAL) { AutoReset = true };
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
        }

        #endregion
    }
}
