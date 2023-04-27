using System;

namespace Artemis.UI.Windows.Utilities;

public static class WindowUtilities
{
    public static int GetActiveProcessId()
    {
        // Get foreground window handle
        IntPtr hWnd = User32.GetForegroundWindow();

        User32.GetWindowThreadProcessId(hWnd, out uint processId);
        return (int) processId;
    }
}