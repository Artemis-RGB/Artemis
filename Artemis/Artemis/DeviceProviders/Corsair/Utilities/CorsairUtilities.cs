using System.Collections.Generic;
using System.Windows.Forms;
using CUE.NET.Devices.Generic.Enums;

namespace Artemis.DeviceProviders.Corsair.Utilities
{
    public static class CorsairUtilities
    {
        static CorsairUtilities()
        {
            FormsKeys = new Dictionary<Keys, CorsairLedId>
            {
                {Keys.Scroll, CorsairLedId.ScrollLock},
                {Keys.Pause, CorsairLedId.PauseBreak},
                {Keys.Back, CorsairLedId.Backspace},
                {Keys.Oemtilde, CorsairLedId.GraveAccentAndTilde},
                {Keys.OemMinus, CorsairLedId.MinusAndUnderscore},
                {Keys.Oemplus, CorsairLedId.EqualsAndPlus},
                {Keys.OemOpenBrackets, CorsairLedId.BracketLeft},
                {Keys.Oem6, CorsairLedId.BracketRight},
                {Keys.Return, CorsairLedId.Enter},
                {Keys.Next, CorsairLedId.PageDown},
                {Keys.Capital, CorsairLedId.CapsLock},
                {Keys.Oem1, CorsairLedId.SemicolonAndColon},
                {Keys.Oem7, CorsairLedId.ApostropheAndDoubleQuote},
                {Keys.OemBackslash, CorsairLedId.NonUsBackslash},
                {Keys.LShiftKey, CorsairLedId.LeftShift},
                {Keys.Oem5, CorsairLedId.NonUsTilde},
                {Keys.Oemcomma, CorsairLedId.CommaAndLessThan},
                {Keys.OemPeriod, CorsairLedId.PeriodAndBiggerThan},
                {Keys.OemQuestion, CorsairLedId.SlashAndQuestionMark},
                {Keys.RShiftKey, CorsairLedId.RightShift},
                {Keys.LControlKey, CorsairLedId.LeftCtrl},
                {Keys.LWin, CorsairLedId.LeftGui},
                {Keys.LMenu, CorsairLedId.LeftAlt},
                {Keys.RMenu, CorsairLedId.RightAlt},
                {Keys.RWin, CorsairLedId.RightGui},
                {Keys.Apps, CorsairLedId.Application},
                {Keys.RControlKey, CorsairLedId.RightCtrl},
                {Keys.Left, CorsairLedId.LeftArrow},
                {Keys.Down, CorsairLedId.DownArrow},
                {Keys.Right, CorsairLedId.RightArrow},
                {Keys.Up, CorsairLedId.UpArrow},
                {Keys.NumPad0, CorsairLedId.Keypad0},
                {Keys.NumPad1, CorsairLedId.Keypad1},
                {Keys.NumPad2, CorsairLedId.Keypad2},
                {Keys.NumPad3, CorsairLedId.Keypad3},
                {Keys.NumPad4, CorsairLedId.Keypad4},
                {Keys.NumPad5, CorsairLedId.Keypad5},
                {Keys.NumPad6, CorsairLedId.Keypad6},
                {Keys.NumPad7, CorsairLedId.Keypad7},
                {Keys.NumPad8, CorsairLedId.Keypad8},
                {Keys.NumPad9, CorsairLedId.Keypad9},
                {Keys.Divide, CorsairLedId.KeypadSlash},
                {Keys.Multiply, CorsairLedId.KeypadAsterisk},
                {Keys.Subtract, CorsairLedId.KeypadMinus},
                {Keys.Add, CorsairLedId.KeypadPlus},
                {Keys.Decimal, CorsairLedId.KeypadPeriodAndDelete},
                {Keys.MediaStop, CorsairLedId.Stop},
                {Keys.MediaPreviousTrack, CorsairLedId.ScanPreviousTrack},
                {Keys.MediaNextTrack, CorsairLedId.ScanNextTrack},
                {Keys.MediaPlayPause, CorsairLedId.PlayPause},
                {Keys.VolumeMute, CorsairLedId.Mute},
                {Keys.VolumeUp, CorsairLedId.VolumeUp},
                {Keys.VolumeDown, CorsairLedId.VolumeDown}
            };
        }

        public static Dictionary<Keys, CorsairLedId> FormsKeys { get; set; }
        public static object SDKLock { get; set; } = new object();
    }
}