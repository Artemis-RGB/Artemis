using System.Collections.Generic;
using System.Windows.Forms;
using CUE.NET.Devices.Keyboard.Enums;

namespace Artemis.DeviceProviders.Corsair.Utilities
{
    public static class KeyMap
    {
        static KeyMap()
        {
            FormsKeys = new Dictionary<Keys, CorsairKeyboardKeyId>
            {
                {Keys.Scroll, CorsairKeyboardKeyId.ScrollLock},
                {Keys.Pause, CorsairKeyboardKeyId.PauseBreak},
                {Keys.Back, CorsairKeyboardKeyId.Backspace},
                {Keys.Oemtilde, CorsairKeyboardKeyId.GraveAccentAndTilde},
                {Keys.OemMinus, CorsairKeyboardKeyId.MinusAndUnderscore},
                {Keys.Oemplus, CorsairKeyboardKeyId.EqualsAndPlus},
                {Keys.OemOpenBrackets, CorsairKeyboardKeyId.BracketLeft},
                {Keys.Oem6, CorsairKeyboardKeyId.BracketRight},
                {Keys.Return, CorsairKeyboardKeyId.Enter},
                {Keys.Next, CorsairKeyboardKeyId.PageDown},
                {Keys.Capital, CorsairKeyboardKeyId.CapsLock},
                {Keys.Oem1, CorsairKeyboardKeyId.SemicolonAndColon},
                {Keys.Oem7, CorsairKeyboardKeyId.ApostropheAndDoubleQuote},
                {Keys.OemBackslash, CorsairKeyboardKeyId.Backslash},
                {Keys.LShiftKey, CorsairKeyboardKeyId.LeftShift},
                {Keys.Oem5, CorsairKeyboardKeyId.NonUsBackslash},
                {Keys.Oemcomma, CorsairKeyboardKeyId.CommaAndLessThan},
                {Keys.OemPeriod, CorsairKeyboardKeyId.PeriodAndBiggerThan},
                {Keys.OemQuestion, CorsairKeyboardKeyId.SlashAndQuestionMark},
                {Keys.RShiftKey, CorsairKeyboardKeyId.RightShift},
                {Keys.LControlKey, CorsairKeyboardKeyId.LeftCtrl},
                {Keys.LWin, CorsairKeyboardKeyId.LeftGui},
                {Keys.LMenu, CorsairKeyboardKeyId.LeftAlt},
                {Keys.RMenu, CorsairKeyboardKeyId.RightAlt},
                {Keys.RWin, CorsairKeyboardKeyId.RightGui},
                {Keys.Apps, CorsairKeyboardKeyId.Application},
                {Keys.RControlKey, CorsairKeyboardKeyId.RightCtrl},
                {Keys.Left, CorsairKeyboardKeyId.LeftArrow},
                {Keys.Down, CorsairKeyboardKeyId.DownArrow},
                {Keys.Right, CorsairKeyboardKeyId.RightArrow},
                {Keys.Up, CorsairKeyboardKeyId.UpArrow},
                {Keys.NumPad0, CorsairKeyboardKeyId.Keypad0},
                {Keys.NumPad1, CorsairKeyboardKeyId.Keypad1},
                {Keys.NumPad2, CorsairKeyboardKeyId.Keypad2},
                {Keys.NumPad3, CorsairKeyboardKeyId.Keypad3},
                {Keys.NumPad4, CorsairKeyboardKeyId.Keypad4},
                {Keys.NumPad5, CorsairKeyboardKeyId.Keypad5},
                {Keys.NumPad6, CorsairKeyboardKeyId.Keypad6},
                {Keys.NumPad7, CorsairKeyboardKeyId.Keypad7},
                {Keys.NumPad8, CorsairKeyboardKeyId.Keypad8},
                {Keys.NumPad9, CorsairKeyboardKeyId.Keypad9},
                {Keys.Divide, CorsairKeyboardKeyId.KeypadSlash},
                {Keys.Multiply, CorsairKeyboardKeyId.KeypadAsterisk},
                {Keys.Subtract, CorsairKeyboardKeyId.KeypadMinus},
                {Keys.Add, CorsairKeyboardKeyId.KeypadPlus},
                {Keys.Decimal, CorsairKeyboardKeyId.KeypadPeriodAndDelete},
                {Keys.MediaStop, CorsairKeyboardKeyId.Stop},
                {Keys.MediaPreviousTrack, CorsairKeyboardKeyId.ScanPreviousTrack},
                {Keys.MediaNextTrack, CorsairKeyboardKeyId.ScanNextTrack},
                {Keys.MediaPlayPause, CorsairKeyboardKeyId.PlayPause},
                {Keys.VolumeMute, CorsairKeyboardKeyId.Mute},
                {Keys.VolumeUp, CorsairKeyboardKeyId.VolumeUp},
                {Keys.VolumeDown, CorsairKeyboardKeyId.VolumeDown}
            };
        }

        public static Dictionary<Keys, CorsairKeyboardKeyId> FormsKeys { get; set; }
    }
}