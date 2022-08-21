using System;

namespace Artemis.UI.Windows.Providers.Input;

public class SpongeWindowEventArgs : EventArgs
{
    public SpongeWindowEventArgs(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        HWnd = hWnd;
        Msg = msg;
        WParam = wParam;
        LParam = lParam;
    }

    public IntPtr HWnd { get; }
    public uint Msg { get; }
    public IntPtr WParam { get; }
    public IntPtr LParam { get; }
}