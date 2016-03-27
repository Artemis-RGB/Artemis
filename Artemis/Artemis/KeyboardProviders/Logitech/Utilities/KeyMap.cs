using System.Collections.Generic;
using System.Windows.Forms;
using Artemis.Utilities.Keyboard;

namespace Artemis.KeyboardProviders.Logitech.Utilities
{
    public static class KeyMap
    {
        static KeyMap()
        {
            // There are several keyboard layouts
            // TODO: Implemented more layouts and an option to select them
            UsEnglishOrionKeys = new List<Key>
            {
                // Row 1
                new Key(Keys.Escape, 0, 0),
                new Key(Keys.F1, 1, 0),
                new Key(Keys.F2, 2, 0),
                new Key(Keys.F3, 3, 0),
                new Key(Keys.F4, 4, 0),
                new Key(Keys.F5, 5, 0),
                new Key(Keys.F6, 6, 0),
                new Key(Keys.F7, 7, 0),
                new Key(Keys.F8, 8, 0),
                new Key(Keys.F9, 9, 0),
                new Key(Keys.F10, 10, 0),
                new Key(Keys.F11, 11, 0),
                new Key(Keys.F12, 12, 0),
                new Key(Keys.PrintScreen, 13, 0),
                new Key(Keys.Scroll, 14, 0),
                new Key(Keys.Pause, 15, 0),

                // Row 2
                new Key(Keys.Oemtilde, 0, 1),
                new Key(Keys.D1, 1, 1),
                new Key(Keys.D2, 2, 1),
                new Key(Keys.D3, 3, 1),
                new Key(Keys.D4, 4, 1),
                new Key(Keys.D5, 5, 1),
                new Key(Keys.D6, 6, 1),
                new Key(Keys.D7, 7, 1),
                new Key(Keys.D8, 8, 1),
                new Key(Keys.D9, 9, 1),
                new Key(Keys.D0, 10, 1),
                new Key(Keys.OemMinus, 11, 1),
                new Key(Keys.Oemplus, 12, 1),
                new Key(Keys.Back, 13, 1),
                new Key(Keys.Insert, 14, 1),
                new Key(Keys.Home, 15, 1),
                new Key(Keys.PageUp, 16, 1),
                new Key(Keys.NumLock, 17, 1),
                new Key(Keys.Divide, 18, 1),
                new Key(Keys.Multiply, 19, 1),
                new Key(Keys.Subtract, 20, 1),

                // Row 3
                new Key(Keys.Tab, 0, 2),
                new Key(Keys.Q, 1, 2),
                new Key(Keys.W, 2, 2),
                new Key(Keys.E, 3, 2),
                new Key(Keys.R, 4, 2),
                new Key(Keys.T, 5, 2),
                new Key(Keys.Y, 6, 2),
                new Key(Keys.U, 7, 2),
                new Key(Keys.I, 8, 2),
                new Key(Keys.O, 9, 2),
                new Key(Keys.P, 10, 2),
                new Key(Keys.OemOpenBrackets, 11, 2),
                new Key(Keys.Oem6, 12, 2),
                new Key(Keys.Delete, 14, 2),
                new Key(Keys.End, 15, 2),
                new Key(Keys.Next, 16, 2),
                new Key(Keys.NumPad7, 17, 2),
                new Key(Keys.NumPad8, 18, 2),
                new Key(Keys.NumPad9, 19, 2),
                new Key(Keys.Add, 20, 2),

                // Row 4
                new Key(Keys.Capital, 0, 3),
                new Key(Keys.A, 1, 3),
                new Key(Keys.S, 2, 3),
                new Key(Keys.D, 3, 3),
                new Key(Keys.F, 4, 3),
                new Key(Keys.G, 5, 3),
                new Key(Keys.H, 6, 3),
                new Key(Keys.J, 7, 3),
                new Key(Keys.K, 8, 3),
                new Key(Keys.L, 9, 3),
                new Key(Keys.Oem1, 10, 3),
                new Key(Keys.Oem7, 11, 3),
                new Key(Keys.Oem5, 12, 3),
                new Key(Keys.Return, 13, 3),
                new Key(Keys.NumPad4, 17, 3),
                new Key(Keys.NumPad5, 18, 3),
                new Key(Keys.NumPad6, 19, 3),

                // Row 5
                new Key(Keys.LShiftKey, 1, 4),
                new Key(Keys.OemBackslash, 2, 4),
                new Key(Keys.Z, 2, 4),
                new Key(Keys.X, 3, 4),
                new Key(Keys.C, 4, 4),
                new Key(Keys.V, 5, 4),
                new Key(Keys.B, 6, 4),
                new Key(Keys.N, 7, 4),
                new Key(Keys.M, 8, 4),
                new Key(Keys.Oemcomma, 9, 4),
                new Key(Keys.OemPeriod, 10, 4),
                new Key(Keys.OemQuestion, 11, 4),
                new Key(Keys.RShiftKey, 13, 4),
                new Key(Keys.Up, 15, 4),
                new Key(Keys.NumPad1, 17, 4),
                new Key(Keys.NumPad2, 18, 4),
                new Key(Keys.NumPad3, 19, 4),
                // Both returns return "Return" (Yes...)
                // new OrionKey(System.Windows.Forms.Keys.Return, 20, 4),

                // Row 6
                new Key(Keys.LControlKey, 0, 5),
                new Key(Keys.LWin, 1, 5),
                new Key(Keys.LMenu, 2, 5),
                new Key(Keys.Space, 5, 5),
                new Key(Keys.RMenu, 11, 5),
                new Key(Keys.RWin, 12, 5),
                new Key(Keys.Apps, 13, 5),
                new Key(Keys.RControlKey, 14, 5),
                new Key(Keys.Left, 15, 5),
                new Key(Keys.Down, 16, 5),
                new Key(Keys.Right, 17, 5),
                new Key(Keys.NumPad0, 18, 5),
                new Key(Keys.Decimal, 19, 5)
            };
        }

        public static List<Key> UsEnglishOrionKeys { get; set; }
    }
}