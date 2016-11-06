using System.Runtime.InteropServices;
using System.Windows;
using MoonSharp.Interpreter;

namespace Artemis.Profiles
{
    [MoonSharpUserData]
    public class LuaMouseWrapper
    {
        public int Y => (int) GetMousePosition().Y;

        public int X => (int) GetMousePosition().X;


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        public static Point GetMousePosition()
        {
            var w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public int X;
            public int Y;
        }
    }
}