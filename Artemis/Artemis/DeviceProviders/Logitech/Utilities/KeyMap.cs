using System.Collections.Generic;
using System.Windows.Forms;

namespace Artemis.DeviceProviders.Logitech.Utilities
{
    public static class KeyMap
    {
        static KeyMap()
        {
            // There are several keyboard layouts
            // TODO: Implemented more layouts and an option to select them
            UsEnglishOrionKeys = new List<KeyMatch>
            {
                // Row 1
                new KeyMatch(Keys.Escape, 0, 0),
                new KeyMatch(Keys.F1, 1, 0),
                new KeyMatch(Keys.F2, 2, 0),
                new KeyMatch(Keys.F3, 3, 0),
                new KeyMatch(Keys.F4, 4, 0),
                new KeyMatch(Keys.F5, 5, 0),
                new KeyMatch(Keys.F6, 6, 0),
                new KeyMatch(Keys.F7, 7, 0),
                new KeyMatch(Keys.F8, 8, 0),
                new KeyMatch(Keys.F9, 9, 0),
                new KeyMatch(Keys.F10, 10, 0),
                new KeyMatch(Keys.F11, 11, 0),
                new KeyMatch(Keys.F12, 12, 0),
                new KeyMatch(Keys.PrintScreen, 13, 0),
                new KeyMatch(Keys.Scroll, 14, 0),
                new KeyMatch(Keys.Pause, 15, 0),

                // Row 2
                new KeyMatch(Keys.Oemtilde, 0, 1),
                new KeyMatch(Keys.D1, 1, 1),
                new KeyMatch(Keys.D2, 2, 1),
                new KeyMatch(Keys.D3, 3, 1),
                new KeyMatch(Keys.D4, 4, 1),
                new KeyMatch(Keys.D5, 5, 1),
                new KeyMatch(Keys.D6, 6, 1),
                new KeyMatch(Keys.D7, 7, 1),
                new KeyMatch(Keys.D8, 8, 1),
                new KeyMatch(Keys.D9, 9, 1),
                new KeyMatch(Keys.D0, 10, 1),
                new KeyMatch(Keys.OemMinus, 11, 1),
                new KeyMatch(Keys.Oemplus, 12, 1),
                new KeyMatch(Keys.Back, 13, 1),
                new KeyMatch(Keys.Insert, 14, 1),
                new KeyMatch(Keys.Home, 15, 1),
                new KeyMatch(Keys.PageUp, 16, 1),
                new KeyMatch(Keys.NumLock, 17, 1),
                new KeyMatch(Keys.Divide, 18, 1),
                new KeyMatch(Keys.Multiply, 19, 1),
                new KeyMatch(Keys.Subtract, 20, 1),

                // Row 3
                new KeyMatch(Keys.Tab, 0, 2),
                new KeyMatch(Keys.Q, 1, 2),
                new KeyMatch(Keys.W, 2, 2),
                new KeyMatch(Keys.E, 3, 2),
                new KeyMatch(Keys.R, 4, 2),
                new KeyMatch(Keys.T, 5, 2),
                new KeyMatch(Keys.Y, 6, 2),
                new KeyMatch(Keys.U, 7, 2),
                new KeyMatch(Keys.I, 8, 2),
                new KeyMatch(Keys.O, 9, 2),
                new KeyMatch(Keys.P, 10, 2),
                new KeyMatch(Keys.OemOpenBrackets, 11, 2),
                new KeyMatch(Keys.Oem6, 12, 2),
                new KeyMatch(Keys.Delete, 14, 2),
                new KeyMatch(Keys.End, 15, 2),
                new KeyMatch(Keys.Next, 16, 2),
                new KeyMatch(Keys.NumPad7, 17, 2),
                new KeyMatch(Keys.NumPad8, 18, 2),
                new KeyMatch(Keys.NumPad9, 19, 2),
                new KeyMatch(Keys.Add, 20, 2),

                // Row 4
                new KeyMatch(Keys.Capital, 0, 3),
                new KeyMatch(Keys.A, 1, 3),
                new KeyMatch(Keys.S, 2, 3),
                new KeyMatch(Keys.D, 3, 3),
                new KeyMatch(Keys.F, 4, 3),
                new KeyMatch(Keys.G, 5, 3),
                new KeyMatch(Keys.H, 6, 3),
                new KeyMatch(Keys.J, 7, 3),
                new KeyMatch(Keys.K, 8, 3),
                new KeyMatch(Keys.L, 9, 3),
                new KeyMatch(Keys.Oem1, 10, 3),
                new KeyMatch(Keys.Oem7, 11, 3),
                new KeyMatch(Keys.Oem5, 12, 3),
                new KeyMatch(Keys.Return, 13, 3),
                new KeyMatch(Keys.NumPad4, 17, 3),
                new KeyMatch(Keys.NumPad5, 18, 3),
                new KeyMatch(Keys.NumPad6, 19, 3),

                // Row 5
                new KeyMatch(Keys.LShiftKey, 1, 4),
                new KeyMatch(Keys.OemBackslash, 2, 4),
                new KeyMatch(Keys.Z, 2, 4),
                new KeyMatch(Keys.X, 3, 4),
                new KeyMatch(Keys.C, 4, 4),
                new KeyMatch(Keys.V, 5, 4),
                new KeyMatch(Keys.B, 6, 4),
                new KeyMatch(Keys.N, 7, 4),
                new KeyMatch(Keys.M, 8, 4),
                new KeyMatch(Keys.Oemcomma, 9, 4),
                new KeyMatch(Keys.OemPeriod, 10, 4),
                new KeyMatch(Keys.OemQuestion, 11, 4),
                new KeyMatch(Keys.RShiftKey, 13, 4),
                new KeyMatch(Keys.Up, 15, 4),
                new KeyMatch(Keys.NumPad1, 17, 4),
                new KeyMatch(Keys.NumPad2, 18, 4),
                new KeyMatch(Keys.NumPad3, 19, 4),
                // Both returns return "Return" (Yes...)
                // new OrionKey(System.Windows.Forms.Keys.Return, 20, 4),

                // Row 6
                new KeyMatch(Keys.LControlKey, 0, 5),
                new KeyMatch(Keys.LWin, 1, 5),
                new KeyMatch(Keys.LMenu, 2, 5),
                new KeyMatch(Keys.Space, 5, 5),
                new KeyMatch(Keys.RMenu, 11, 5),
                new KeyMatch(Keys.RWin, 12, 5),
                new KeyMatch(Keys.Apps, 13, 5),
                new KeyMatch(Keys.RControlKey, 14, 5),
                new KeyMatch(Keys.Left, 15, 5),
                new KeyMatch(Keys.Down, 16, 5),
                new KeyMatch(Keys.Right, 17, 5),
                new KeyMatch(Keys.NumPad0, 18, 5),
                new KeyMatch(Keys.Decimal, 19, 5)
            };
        }

        public static List<KeyMatch> UsEnglishOrionKeys { get; set; }
    }
}