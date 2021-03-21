using System;
using System.Runtime.InteropServices;

namespace Artemis.UI.SkiaSharp
{
    internal class Win32Window : IDisposable
    {
        private ushort classRegistration;

        public string WindowClassName { get; }

        public IntPtr WindowHandle { get; private set; }

        public IntPtr DeviceContextHandle { get; private set; }

        public Win32Window(string className)
        {
            WindowClassName = className;

            var wc = new WNDCLASS
            {
                cbClsExtra = 0,
                cbWndExtra = 0,
                hbrBackground = IntPtr.Zero,
                hCursor = User32.LoadCursor(IntPtr.Zero, (int)User32.IDC_ARROW),
                hIcon = User32.LoadIcon(IntPtr.Zero, (IntPtr)User32.IDI_APPLICATION),
                hInstance = Kernel32.CurrentModuleHandle,
                lpfnWndProc = (WNDPROC)User32.DefWindowProc,
                lpszClassName = WindowClassName,
                lpszMenuName = null,
                style = User32.CS_HREDRAW | User32.CS_VREDRAW | User32.CS_OWNDC
            };

            classRegistration = User32.RegisterClass(ref wc);
            if (classRegistration == 0)
                throw new Exception($"Could not register window class: {className}");

            WindowHandle = User32.CreateWindow(
                WindowClassName,
                $"The Invisible Man ({className})",
                User32.WindowStyles.WS_OVERLAPPEDWINDOW,
                0, 0,
                1, 1,
                IntPtr.Zero, IntPtr.Zero, Kernel32.CurrentModuleHandle, IntPtr.Zero);
            if (WindowHandle == IntPtr.Zero)
                throw new Exception($"Could not create window: {className}");

            DeviceContextHandle = User32.GetDC(WindowHandle);
            if (DeviceContextHandle == IntPtr.Zero)
            {
                Dispose();
                throw new Exception($"Could not get device context: {className}");
            }
        }

        public void Dispose()
        {
            if (WindowHandle != IntPtr.Zero)
            {
                if (DeviceContextHandle != IntPtr.Zero)
                {
                    User32.ReleaseDC(WindowHandle, DeviceContextHandle);
                    DeviceContextHandle = IntPtr.Zero;
                }

                User32.DestroyWindow(WindowHandle);
                WindowHandle = IntPtr.Zero;
            }

            if (classRegistration != 0)
            {
                User32.UnregisterClass(WindowClassName, Kernel32.CurrentModuleHandle);
                classRegistration = 0;
            }
        }

        public delegate IntPtr WNDPROC(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct WNDCLASS
        {
            public uint style;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public WNDPROC lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpszMenuName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpszClassName;
        }
    }
}
