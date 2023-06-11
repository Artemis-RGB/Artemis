using System;
using System.Runtime.InteropServices;
using System.Text;
using Artemis.Core.Services;
using Microsoft.Win32;

namespace Artemis.UI.Windows.Utilities;

/// <summary>
///     Provides static methods to convert between Win32 VirtualKeys
///     and our Key enum.
/// </summary>
public static class InputUtilities
{
    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    private static extern short GetKeyState(int keyCode);

    [Flags]
    private enum KeyStates
    {
        None = 0,
        Down = 1,
        Toggled = 2
    }

    private readonly record struct KeystrokeInfo(int ScanCode, int VirtualKey, bool IsE0, bool IsE1);
    
    /// <summary>
    ///     https://blog.molecular-matters.com/2011/09/05/properly-handling-keyboard-input/
    /// </summary>
    public static KeyboardKey CorrectVirtualKeyAndScanCode(int virtualKey, int scanCode, uint flags)
    {
        KeystrokeInfo info = new()
        {
            ScanCode = scanCode,
            VirtualKey = virtualKey,
            IsE0 = (flags & 2) != 0,
            IsE1 = (flags & 4) != 0
        };

        if (info.ScanCode == 0)
        {
            // These keys are media or browser keys, they don't have a scan code
            return KeyFromVirtualKey(info.VirtualKey);
        }

        return info switch
        {
            // Fake keys, usually escape sequences
            { VirtualKey: 255 } => KeyboardKey.None,
            // AltGr
            { ScanCode: 56, VirtualKey: NativeMethods.VK_CONTROL, IsE0: true } => KeyboardKey.None,
            
            { ScanCode: 28, IsE0: true } => KeyboardKey.NumPadEnter,
            { ScanCode: 28, IsE0: false } => KeyboardKey.Enter,
            { ScanCode: 29, IsE1: true } => KeyboardKey.PauseBreak,
            { ScanCode: 29, IsE0: true } => KeyboardKey.RightCtrl,
            { ScanCode: 29, IsE0: false } => KeyboardKey.LeftCtrl,
            { ScanCode: 56, IsE0: true } => KeyboardKey.RightAlt,
            { ScanCode: 56, IsE0: false } => KeyboardKey.LeftAlt,
            { ScanCode: 53, IsE0: true } => KeyboardKey.NumPadDivide,
            { ScanCode: 53, IsE0: false } => KeyboardKey.OemQuestion,
            { ScanCode: 55, IsE0: true } => KeyboardKey.PrintScreen,
            { ScanCode: 55, IsE0: false } => KeyboardKey.NumPadMultiply,
            { ScanCode: 71, IsE0: true } => KeyboardKey.Home,
            { ScanCode: 71, IsE0: false } => KeyboardKey.NumPad7,
            { ScanCode: 72, IsE0: true } => KeyboardKey.ArrowUp,
            { ScanCode: 72, IsE0: false } => KeyboardKey.NumPad8,
            { ScanCode: 73, IsE0: true } => KeyboardKey.PageUp,
            { ScanCode: 73, IsE0: false } => KeyboardKey.NumPad9,
            { ScanCode: 75, IsE0: true } => KeyboardKey.ArrowLeft,
            { ScanCode: 75, IsE0: false } => KeyboardKey.NumPad4,
            { ScanCode: 76, IsE0: true } => KeyboardKey.Clear,
            { ScanCode: 76, IsE0: false } => KeyboardKey.NumPad5,
            { ScanCode: 77, IsE0: true } => KeyboardKey.ArrowRight,
            { ScanCode: 77, IsE0: false } => KeyboardKey.NumPad6,
            { ScanCode: 79, IsE0: true } => KeyboardKey.End,
            { ScanCode: 79, IsE0: false } => KeyboardKey.NumPad1,
            { ScanCode: 80, IsE0: true } => KeyboardKey.ArrowDown,
            { ScanCode: 80, IsE0: false } => KeyboardKey.NumPad2,
            { ScanCode: 81, IsE0: true } => KeyboardKey.PageDown,
            { ScanCode: 81, IsE0: false } => KeyboardKey.NumPad3,
            { ScanCode: 82, IsE0: true } => KeyboardKey.Insert,
            { ScanCode: 82, IsE0: false } => KeyboardKey.NumPad0,
            { ScanCode: 83, IsE0: true } => KeyboardKey.Delete,
            { ScanCode: 83, IsE0: false } => KeyboardKey.NumPadDecimal,
            _ => KeyFromScanCode((uint)info.ScanCode),
        };
    }

    public static bool IsKeyToggled(KeyboardKey key)
    {
        return KeyStates.Toggled == (GetKeyState(key) & KeyStates.Toggled);
    }

    private static KeyStates GetKeyState(KeyboardKey key)
    {
        KeyStates state = KeyStates.None;

        short retVal = GetKeyState(VirtualKeyFromKey(key));

        //If the high-order bit is 1, the key is down
        //otherwise, it is up.
        if ((retVal & 0x8000) == 0x8000)
            state |= KeyStates.Down;

        //If the low-order bit is 1, the key is toggled.
        if ((retVal & 1) == 1)
            state |= KeyStates.Toggled;

        return state;
    }

    /// <summary>
    ///     Convert a Win32 VirtualKey into our Key enum.
    /// </summary>
    public static KeyboardKey KeyFromVirtualKey(int virtualKey)
    {
        return virtualKey switch
        {
            NativeMethods.VK_BACK => KeyboardKey.Backspace,
            NativeMethods.VK_TAB => KeyboardKey.Tab,
            NativeMethods.VK_CLEAR => KeyboardKey.Clear,
            NativeMethods.VK_RETURN => KeyboardKey.Enter,
            NativeMethods.VK_PAUSE => KeyboardKey.PauseBreak,
            NativeMethods.VK_CAPSLOCK => KeyboardKey.CapsLock,
            NativeMethods.VK_ESCAPE => KeyboardKey.Escape,
            NativeMethods.VK_SPACE => KeyboardKey.Space,
            NativeMethods.VK_PRIOR => KeyboardKey.PageUp,
            NativeMethods.VK_NEXT => KeyboardKey.PageDown,
            NativeMethods.VK_END => KeyboardKey.End,
            NativeMethods.VK_HOME => KeyboardKey.Home,
            NativeMethods.VK_LEFT => KeyboardKey.ArrowLeft,
            NativeMethods.VK_UP => KeyboardKey.ArrowUp,
            NativeMethods.VK_RIGHT => KeyboardKey.ArrowRight,
            NativeMethods.VK_DOWN => KeyboardKey.ArrowDown,
            NativeMethods.VK_PRINT => KeyboardKey.PrintScreen,
            NativeMethods.VK_INSERT => KeyboardKey.Insert,
            NativeMethods.VK_DELETE => KeyboardKey.Delete,
            NativeMethods.VK_0 => KeyboardKey.D0,
            NativeMethods.VK_1 => KeyboardKey.D1,
            NativeMethods.VK_2 => KeyboardKey.D2,
            NativeMethods.VK_3 => KeyboardKey.D3,
            NativeMethods.VK_4 => KeyboardKey.D4,
            NativeMethods.VK_5 => KeyboardKey.D5,
            NativeMethods.VK_6 => KeyboardKey.D6,
            NativeMethods.VK_7 => KeyboardKey.D7,
            NativeMethods.VK_8 => KeyboardKey.D8,
            NativeMethods.VK_9 => KeyboardKey.D9,
            NativeMethods.VK_A => KeyboardKey.A,
            NativeMethods.VK_B => KeyboardKey.B,
            NativeMethods.VK_C => KeyboardKey.C,
            NativeMethods.VK_D => KeyboardKey.D,
            NativeMethods.VK_E => KeyboardKey.E,
            NativeMethods.VK_F => KeyboardKey.F,
            NativeMethods.VK_G => KeyboardKey.G,
            NativeMethods.VK_H => KeyboardKey.H,
            NativeMethods.VK_I => KeyboardKey.I,
            NativeMethods.VK_J => KeyboardKey.J,
            NativeMethods.VK_K => KeyboardKey.K,
            NativeMethods.VK_L => KeyboardKey.L,
            NativeMethods.VK_M => KeyboardKey.M,
            NativeMethods.VK_N => KeyboardKey.N,
            NativeMethods.VK_O => KeyboardKey.O,
            NativeMethods.VK_P => KeyboardKey.P,
            NativeMethods.VK_Q => KeyboardKey.Q,
            NativeMethods.VK_R => KeyboardKey.R,
            NativeMethods.VK_S => KeyboardKey.S,
            NativeMethods.VK_T => KeyboardKey.T,
            NativeMethods.VK_U => KeyboardKey.U,
            NativeMethods.VK_V => KeyboardKey.V,
            NativeMethods.VK_W => KeyboardKey.W,
            NativeMethods.VK_X => KeyboardKey.X,
            NativeMethods.VK_Y => KeyboardKey.Y,
            NativeMethods.VK_Z => KeyboardKey.Z,
            NativeMethods.VK_LWIN => KeyboardKey.LeftWin,
            NativeMethods.VK_RWIN => KeyboardKey.RightWin,
            NativeMethods.VK_APPS => KeyboardKey.Application,
            NativeMethods.VK_SLEEP => KeyboardKey.Sleep,
            NativeMethods.VK_NUMPAD0 => KeyboardKey.NumPad0,
            NativeMethods.VK_NUMPAD1 => KeyboardKey.NumPad1,
            NativeMethods.VK_NUMPAD2 => KeyboardKey.NumPad2,
            NativeMethods.VK_NUMPAD3 => KeyboardKey.NumPad3,
            NativeMethods.VK_NUMPAD4 => KeyboardKey.NumPad4,
            NativeMethods.VK_NUMPAD5 => KeyboardKey.NumPad5,
            NativeMethods.VK_NUMPAD6 => KeyboardKey.NumPad6,
            NativeMethods.VK_NUMPAD7 => KeyboardKey.NumPad7,
            NativeMethods.VK_NUMPAD8 => KeyboardKey.NumPad8,
            NativeMethods.VK_NUMPAD9 => KeyboardKey.NumPad9,
            NativeMethods.VK_MULTIPLY => KeyboardKey.NumPadMultiply,
            NativeMethods.VK_ADD => KeyboardKey.NumPadAdd,
            NativeMethods.VK_SEPARATOR => KeyboardKey.NumPadSeparator,
            NativeMethods.VK_SUBTRACT => KeyboardKey.NumPadSubtract,
            NativeMethods.VK_DECIMAL => KeyboardKey.NumPadDecimal,
            NativeMethods.VK_DIVIDE => KeyboardKey.NumPadDivide,
            NativeMethods.VK_F1 => KeyboardKey.F1,
            NativeMethods.VK_F2 => KeyboardKey.F2,
            NativeMethods.VK_F3 => KeyboardKey.F3,
            NativeMethods.VK_F4 => KeyboardKey.F4,
            NativeMethods.VK_F5 => KeyboardKey.F5,
            NativeMethods.VK_F6 => KeyboardKey.F6,
            NativeMethods.VK_F7 => KeyboardKey.F7,
            NativeMethods.VK_F8 => KeyboardKey.F8,
            NativeMethods.VK_F9 => KeyboardKey.F9,
            NativeMethods.VK_F10 => KeyboardKey.F10,
            NativeMethods.VK_F11 => KeyboardKey.F11,
            NativeMethods.VK_F12 => KeyboardKey.F12,
            NativeMethods.VK_F13 => KeyboardKey.F13,
            NativeMethods.VK_F14 => KeyboardKey.F14,
            NativeMethods.VK_F15 => KeyboardKey.F15,
            NativeMethods.VK_F16 => KeyboardKey.F16,
            NativeMethods.VK_F17 => KeyboardKey.F17,
            NativeMethods.VK_F18 => KeyboardKey.F18,
            NativeMethods.VK_F19 => KeyboardKey.F19,
            NativeMethods.VK_F20 => KeyboardKey.F20,
            NativeMethods.VK_F21 => KeyboardKey.F21,
            NativeMethods.VK_F22 => KeyboardKey.F22,
            NativeMethods.VK_F23 => KeyboardKey.F23,
            NativeMethods.VK_F24 => KeyboardKey.F24,
            NativeMethods.VK_NUMLOCK => KeyboardKey.NumLock,
            NativeMethods.VK_SCROLL => KeyboardKey.ScrollLock,
            NativeMethods.VK_SHIFT => KeyboardKey.LeftShift,
            NativeMethods.VK_LSHIFT => KeyboardKey.LeftShift,
            NativeMethods.VK_RSHIFT => KeyboardKey.RightShift,
            NativeMethods.VK_CONTROL => KeyboardKey.LeftCtrl,
            NativeMethods.VK_LCONTROL => KeyboardKey.LeftCtrl,
            NativeMethods.VK_RCONTROL => KeyboardKey.RightCtrl,
            NativeMethods.VK_MENU => KeyboardKey.LeftAlt,
            NativeMethods.VK_LMENU => KeyboardKey.LeftAlt,
            NativeMethods.VK_RMENU => KeyboardKey.RightAlt,
            NativeMethods.VK_BROWSER_BACK => KeyboardKey.BrowserBack,
            NativeMethods.VK_BROWSER_FORWARD => KeyboardKey.BrowserForward,
            NativeMethods.VK_BROWSER_REFRESH => KeyboardKey.BrowserRefresh,
            NativeMethods.VK_BROWSER_STOP => KeyboardKey.BrowserStop,
            NativeMethods.VK_BROWSER_SEARCH => KeyboardKey.BrowserSearch,
            NativeMethods.VK_BROWSER_FAVORITES => KeyboardKey.BrowserFavorites,
            NativeMethods.VK_BROWSER_HOME => KeyboardKey.BrowserHome,
            NativeMethods.VK_VOLUME_MUTE => KeyboardKey.VolumeMute,
            NativeMethods.VK_VOLUME_DOWN => KeyboardKey.VolumeDown,
            NativeMethods.VK_VOLUME_UP => KeyboardKey.VolumeUp,
            NativeMethods.VK_MEDIA_NEXT_TRACK => KeyboardKey.MediaNextTrack,
            NativeMethods.VK_MEDIA_PREV_TRACK => KeyboardKey.MediaPreviousTrack,
            NativeMethods.VK_MEDIA_STOP => KeyboardKey.MediaStop,
            NativeMethods.VK_MEDIA_PLAY_PAUSE => KeyboardKey.MediaPlayPause,
            NativeMethods.VK_LAUNCH_MAIL => KeyboardKey.LaunchMail,
            NativeMethods.VK_LAUNCH_MEDIA_SELECT => KeyboardKey.SelectMedia,
            NativeMethods.VK_LAUNCH_APP1 => KeyboardKey.FileBrowser,
            NativeMethods.VK_LAUNCH_APP2 => KeyboardKey.Calculator,
            NativeMethods.VK_OEM_1 => KeyboardKey.OemSemicolon,
            NativeMethods.VK_OEM_PLUS => KeyboardKey.OemPlus,
            NativeMethods.VK_OEM_COMMA => KeyboardKey.OemComma,
            NativeMethods.VK_OEM_MINUS => KeyboardKey.OemMinus,
            NativeMethods.VK_OEM_PERIOD => KeyboardKey.OemPeriod,
            NativeMethods.VK_OEM_2 => KeyboardKey.OemQuestion,
            NativeMethods.VK_OEM_3 => KeyboardKey.OemTilde,
            NativeMethods.VK_OEM_4 => KeyboardKey.OemOpenBrackets,
            NativeMethods.VK_OEM_5 => KeyboardKey.OemPipe,
            NativeMethods.VK_OEM_6 => KeyboardKey.OemCloseBrackets,
            NativeMethods.VK_OEM_7 => KeyboardKey.OemQuotes,
            NativeMethods.VK_OEM_102 => KeyboardKey.OemBackslash,
            _ => KeyboardKey.None
        };
    }

    /// <summary>
    ///     Convert our Key enum into a Win32 VirtualKeyboardKey.
    /// </summary>
    public static int VirtualKeyFromKey(KeyboardKey key)
    {
        return key switch
        {
            KeyboardKey.Backspace => NativeMethods.VK_BACK,
            KeyboardKey.Tab => NativeMethods.VK_TAB,
            KeyboardKey.Clear => NativeMethods.VK_CLEAR,
            KeyboardKey.Enter => NativeMethods.VK_RETURN,
            KeyboardKey.PauseBreak => NativeMethods.VK_PAUSE,
            KeyboardKey.CapsLock => NativeMethods.VK_CAPITAL,
            KeyboardKey.Escape => NativeMethods.VK_ESCAPE,
            KeyboardKey.Space => NativeMethods.VK_SPACE,
            KeyboardKey.PageUp => NativeMethods.VK_PRIOR,
            KeyboardKey.PageDown => NativeMethods.VK_NEXT,
            KeyboardKey.End => NativeMethods.VK_END,
            KeyboardKey.Home => NativeMethods.VK_HOME,
            KeyboardKey.ArrowLeft => NativeMethods.VK_LEFT,
            KeyboardKey.ArrowUp => NativeMethods.VK_UP,
            KeyboardKey.ArrowRight => NativeMethods.VK_RIGHT,
            KeyboardKey.ArrowDown => NativeMethods.VK_DOWN,
            KeyboardKey.PrintScreen => NativeMethods.VK_PRINT,
            KeyboardKey.Insert => NativeMethods.VK_INSERT,
            KeyboardKey.Delete => NativeMethods.VK_DELETE,
            KeyboardKey.D0 => NativeMethods.VK_0,
            KeyboardKey.D1 => NativeMethods.VK_1,
            KeyboardKey.D2 => NativeMethods.VK_2,
            KeyboardKey.D3 => NativeMethods.VK_3,
            KeyboardKey.D4 => NativeMethods.VK_4,
            KeyboardKey.D5 => NativeMethods.VK_5,
            KeyboardKey.D6 => NativeMethods.VK_6,
            KeyboardKey.D7 => NativeMethods.VK_7,
            KeyboardKey.D8 => NativeMethods.VK_8,
            KeyboardKey.D9 => NativeMethods.VK_9,
            KeyboardKey.A => NativeMethods.VK_A,
            KeyboardKey.B => NativeMethods.VK_B,
            KeyboardKey.C => NativeMethods.VK_C,
            KeyboardKey.D => NativeMethods.VK_D,
            KeyboardKey.E => NativeMethods.VK_E,
            KeyboardKey.F => NativeMethods.VK_F,
            KeyboardKey.G => NativeMethods.VK_G,
            KeyboardKey.H => NativeMethods.VK_H,
            KeyboardKey.I => NativeMethods.VK_I,
            KeyboardKey.J => NativeMethods.VK_J,
            KeyboardKey.K => NativeMethods.VK_K,
            KeyboardKey.L => NativeMethods.VK_L,
            KeyboardKey.M => NativeMethods.VK_M,
            KeyboardKey.N => NativeMethods.VK_N,
            KeyboardKey.O => NativeMethods.VK_O,
            KeyboardKey.P => NativeMethods.VK_P,
            KeyboardKey.Q => NativeMethods.VK_Q,
            KeyboardKey.R => NativeMethods.VK_R,
            KeyboardKey.S => NativeMethods.VK_S,
            KeyboardKey.T => NativeMethods.VK_T,
            KeyboardKey.U => NativeMethods.VK_U,
            KeyboardKey.V => NativeMethods.VK_V,
            KeyboardKey.W => NativeMethods.VK_W,
            KeyboardKey.X => NativeMethods.VK_X,
            KeyboardKey.Y => NativeMethods.VK_Y,
            KeyboardKey.Z => NativeMethods.VK_Z,
            KeyboardKey.LeftWin => NativeMethods.VK_LWIN,
            KeyboardKey.RightWin => NativeMethods.VK_RWIN,
            KeyboardKey.Application => NativeMethods.VK_APPS,
            KeyboardKey.Sleep => NativeMethods.VK_SLEEP,
            KeyboardKey.NumPad0 => NativeMethods.VK_NUMPAD0,
            KeyboardKey.NumPad1 => NativeMethods.VK_NUMPAD1,
            KeyboardKey.NumPad2 => NativeMethods.VK_NUMPAD2,
            KeyboardKey.NumPad3 => NativeMethods.VK_NUMPAD3,
            KeyboardKey.NumPad4 => NativeMethods.VK_NUMPAD4,
            KeyboardKey.NumPad5 => NativeMethods.VK_NUMPAD5,
            KeyboardKey.NumPad6 => NativeMethods.VK_NUMPAD6,
            KeyboardKey.NumPad7 => NativeMethods.VK_NUMPAD7,
            KeyboardKey.NumPad8 => NativeMethods.VK_NUMPAD8,
            KeyboardKey.NumPad9 => NativeMethods.VK_NUMPAD9,
            KeyboardKey.NumPadMultiply => NativeMethods.VK_MULTIPLY,
            KeyboardKey.NumPadAdd => NativeMethods.VK_ADD,
            KeyboardKey.NumPadSeparator => NativeMethods.VK_SEPARATOR,
            KeyboardKey.NumPadSubtract => NativeMethods.VK_SUBTRACT,
            KeyboardKey.NumPadDecimal => NativeMethods.VK_DECIMAL,
            KeyboardKey.NumPadDivide => NativeMethods.VK_DIVIDE,
            KeyboardKey.F1 => NativeMethods.VK_F1,
            KeyboardKey.F2 => NativeMethods.VK_F2,
            KeyboardKey.F3 => NativeMethods.VK_F3,
            KeyboardKey.F4 => NativeMethods.VK_F4,
            KeyboardKey.F5 => NativeMethods.VK_F5,
            KeyboardKey.F6 => NativeMethods.VK_F6,
            KeyboardKey.F7 => NativeMethods.VK_F7,
            KeyboardKey.F8 => NativeMethods.VK_F8,
            KeyboardKey.F9 => NativeMethods.VK_F9,
            KeyboardKey.F10 => NativeMethods.VK_F10,
            KeyboardKey.F11 => NativeMethods.VK_F11,
            KeyboardKey.F12 => NativeMethods.VK_F12,
            KeyboardKey.F13 => NativeMethods.VK_F13,
            KeyboardKey.F14 => NativeMethods.VK_F14,
            KeyboardKey.F15 => NativeMethods.VK_F15,
            KeyboardKey.F16 => NativeMethods.VK_F16,
            KeyboardKey.F17 => NativeMethods.VK_F17,
            KeyboardKey.F18 => NativeMethods.VK_F18,
            KeyboardKey.F19 => NativeMethods.VK_F19,
            KeyboardKey.F20 => NativeMethods.VK_F20,
            KeyboardKey.F21 => NativeMethods.VK_F21,
            KeyboardKey.F22 => NativeMethods.VK_F22,
            KeyboardKey.F23 => NativeMethods.VK_F23,
            KeyboardKey.F24 => NativeMethods.VK_F24,
            KeyboardKey.NumLock => NativeMethods.VK_NUMLOCK,
            KeyboardKey.ScrollLock => NativeMethods.VK_SCROLL,
            KeyboardKey.LeftShift => NativeMethods.VK_LSHIFT,
            KeyboardKey.RightShift => NativeMethods.VK_RSHIFT,
            KeyboardKey.LeftCtrl => NativeMethods.VK_LCONTROL,
            KeyboardKey.RightCtrl => NativeMethods.VK_RCONTROL,
            KeyboardKey.LeftAlt => NativeMethods.VK_LMENU,
            KeyboardKey.RightAlt => NativeMethods.VK_RMENU,
            KeyboardKey.BrowserBack => NativeMethods.VK_BROWSER_BACK,
            KeyboardKey.BrowserForward => NativeMethods.VK_BROWSER_FORWARD,
            KeyboardKey.BrowserRefresh => NativeMethods.VK_BROWSER_REFRESH,
            KeyboardKey.BrowserStop => NativeMethods.VK_BROWSER_STOP,
            KeyboardKey.BrowserSearch => NativeMethods.VK_BROWSER_SEARCH,
            KeyboardKey.BrowserFavorites => NativeMethods.VK_BROWSER_FAVORITES,
            KeyboardKey.BrowserHome => NativeMethods.VK_BROWSER_HOME,
            KeyboardKey.VolumeMute => NativeMethods.VK_VOLUME_MUTE,
            KeyboardKey.VolumeDown => NativeMethods.VK_VOLUME_DOWN,
            KeyboardKey.VolumeUp => NativeMethods.VK_VOLUME_UP,
            KeyboardKey.MediaNextTrack => NativeMethods.VK_MEDIA_NEXT_TRACK,
            KeyboardKey.MediaPreviousTrack => NativeMethods.VK_MEDIA_PREV_TRACK,
            KeyboardKey.MediaStop => NativeMethods.VK_MEDIA_STOP,
            KeyboardKey.MediaPlayPause => NativeMethods.VK_MEDIA_PLAY_PAUSE,
            KeyboardKey.LaunchMail => NativeMethods.VK_LAUNCH_MAIL,
            KeyboardKey.SelectMedia => NativeMethods.VK_LAUNCH_MEDIA_SELECT,
            KeyboardKey.FileBrowser => NativeMethods.VK_LAUNCH_APP1,
            KeyboardKey.Calculator => NativeMethods.VK_LAUNCH_APP2,
            KeyboardKey.OemSemicolon => NativeMethods.VK_OEM_1,
            KeyboardKey.OemPlus => NativeMethods.VK_OEM_PLUS,
            KeyboardKey.OemComma => NativeMethods.VK_OEM_COMMA,
            KeyboardKey.OemMinus => NativeMethods.VK_OEM_MINUS,
            KeyboardKey.OemPeriod => NativeMethods.VK_OEM_PERIOD,
            KeyboardKey.OemQuestion => NativeMethods.VK_OEM_2,
            KeyboardKey.OemTilde => NativeMethods.VK_OEM_3,
            KeyboardKey.OemOpenBrackets => NativeMethods.VK_OEM_4,
            KeyboardKey.OemPipe => NativeMethods.VK_OEM_5,
            KeyboardKey.OemCloseBrackets => NativeMethods.VK_OEM_6,
            KeyboardKey.OemQuotes => NativeMethods.VK_OEM_7,
            KeyboardKey.OemBackslash => NativeMethods.VK_OEM_102,
            _ => 0
        };
    }

    /// <summary>
    ///     Convert a scan code to our Key enum, following US keyboard layout.
    ///     This is useful because we don't care about the keyboard layout, just the key location for effects.
    /// </summary>
    public static KeyboardKey KeyFromScanCode(uint scanCode)
    {
        return scanCode switch
        {
            1 => KeyboardKey.Escape,
            2 => KeyboardKey.D1,
            3 => KeyboardKey.D2,
            4 => KeyboardKey.D3,
            5 => KeyboardKey.D4,
            6 => KeyboardKey.D5,
            7 => KeyboardKey.D6,
            8 => KeyboardKey.D7,
            9 => KeyboardKey.D8,
            10 => KeyboardKey.D9,
            11 => KeyboardKey.D0,
            12 => KeyboardKey.OemMinus,
            13 => KeyboardKey.OemPlus,
            14 => KeyboardKey.Backspace,
            15 => KeyboardKey.Tab,
            16 => KeyboardKey.Q,
            17 => KeyboardKey.W,
            18 => KeyboardKey.E,
            19 => KeyboardKey.R,
            20 => KeyboardKey.T,
            21 => KeyboardKey.Y,
            22 => KeyboardKey.U,
            23 => KeyboardKey.I,
            24 => KeyboardKey.O,
            25 => KeyboardKey.P,
            26 => KeyboardKey.OemOpenBrackets,
            27 => KeyboardKey.OemCloseBrackets,
            30 => KeyboardKey.A,
            31 => KeyboardKey.S,
            32 => KeyboardKey.D,
            33 => KeyboardKey.F,
            34 => KeyboardKey.G,
            35 => KeyboardKey.H,
            36 => KeyboardKey.J,
            37 => KeyboardKey.K,
            38 => KeyboardKey.L,
            39 => KeyboardKey.OemSemicolon,
            40 => KeyboardKey.OemQuotes,
            41 => KeyboardKey.OemTilde,
            42 => KeyboardKey.LeftShift,
            43 => KeyboardKey.OemPipe,
            44 => KeyboardKey.Z,
            45 => KeyboardKey.X,
            46 => KeyboardKey.C,
            47 => KeyboardKey.V,
            48 => KeyboardKey.B,
            49 => KeyboardKey.N,
            50 => KeyboardKey.M,
            51 => KeyboardKey.OemComma,
            52 => KeyboardKey.OemPeriod,
            54 => KeyboardKey.RightShift,
            57 => KeyboardKey.Space,
            58 => KeyboardKey.CapsLock,
            59 => KeyboardKey.F1,
            60 => KeyboardKey.F2,
            61 => KeyboardKey.F3,
            62 => KeyboardKey.F4,
            63 => KeyboardKey.F5,
            64 => KeyboardKey.F6,
            65 => KeyboardKey.F7,
            66 => KeyboardKey.F8,
            67 => KeyboardKey.F9,
            68 => KeyboardKey.F10,
            69 => KeyboardKey.NumLock,
            70 => KeyboardKey.ScrollLock,
            74 => KeyboardKey.NumPadSubtract,
            78 => KeyboardKey.NumPadAdd,
            86 => KeyboardKey.OemBackslash,
            87 => KeyboardKey.F11,
            88 => KeyboardKey.F12,
            91 => KeyboardKey.LeftWin,
            92 => KeyboardKey.RightWin,
            100 => KeyboardKey.F13,
            101 => KeyboardKey.F14,
            102 => KeyboardKey.F15,
            103 => KeyboardKey.F16,
            104 => KeyboardKey.F17,
            105 => KeyboardKey.F18,
            106 => KeyboardKey.F19,
            107 => KeyboardKey.F20,
            108 => KeyboardKey.F21,
            109 => KeyboardKey.F22,
            110 => KeyboardKey.F23,
            //???
            118 => KeyboardKey.F24,
            
            //28 = enter or numpad enter
            //29 = left ctrl or right ctrl
            //53 = numpad slash or slash
            //55 = numpad asterisk or print screen
            //56 = left alt or right alt
            28 or 29 or 53 or 55 or 56 => throw new ArgumentException($"This key is unsupported: {scanCode}", nameof(scanCode)),
            //numpad 789 or home, up, page up
            >= 71 and <= 73 => throw new ArgumentException("Scan code is for a numpad key. These keys are unsupported.", nameof(scanCode)),
            //numpad 456 or left, clear, right
            >= 75 and <= 77 => throw new ArgumentException("Scan code is for a numpad key. These keys are unsupported.", nameof(scanCode)),
            //numpad 1230., or end, down, page down, ins, del
            >= 79 and <= 83 => throw new ArgumentException("Scan code is for a numpad key. These keys are unsupported.", nameof(scanCode)),
            //shouldn't happen, but there might be more weird keys in other layouts
            _ => throw new ArgumentException($"This key is unsupported: {scanCode}", nameof(scanCode)),
        };
    }
}