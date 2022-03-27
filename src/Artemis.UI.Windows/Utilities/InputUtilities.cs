using System;
using System.Runtime.InteropServices;
using Artemis.Core.Services;
using Microsoft.Win32;

namespace Artemis.UI.Windows.Utilities
{
    /// <summary>
    ///     Provides static methods to convert between Win32 VirtualKeys
    ///     and our Key enum.
    /// </summary>
    public static class InputUtilities
    {
        [Flags]
        private enum KeyStates
        {
            None = 0,
            Down = 1,
            Toggled = 2
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern short GetKeyState(int keyCode);

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

        public static bool IsKeyDown(KeyboardKey key)
        {
            return KeyStates.Down == (GetKeyState(key) & KeyStates.Down);
        }

        public static bool IsKeyToggled(KeyboardKey key)
        {
            return KeyStates.Toggled == (GetKeyState(key) & KeyStates.Toggled);
        }

        /// <summary>
        ///     Convert a Win32 VirtualKey into our Key enum.
        /// </summary>
        public static KeyboardKey KeyFromVirtualKey(int virtualKey)
        {
            KeyboardKey key = KeyboardKey.None;

            switch (virtualKey)
            {
                case NativeMethods.VK_CANCEL:
                    key = KeyboardKey.Cancel;
                    break;

                case NativeMethods.VK_BACK:
                    key = KeyboardKey.Back;
                    break;

                case NativeMethods.VK_TAB:
                    key = KeyboardKey.Tab;
                    break;

                case NativeMethods.VK_CLEAR:
                    key = KeyboardKey.Clear;
                    break;

                case NativeMethods.VK_RETURN:
                    key = KeyboardKey.Return;
                    break;

                case NativeMethods.VK_PAUSE:
                    key = KeyboardKey.Pause;
                    break;

                case NativeMethods.VK_CAPSLOCK:
                    key = KeyboardKey.CapsLock;
                    break;

                case NativeMethods.VK_JUNJA:
                    key = KeyboardKey.JunjaMode;
                    break;

                case NativeMethods.VK_FINAL:
                    key = KeyboardKey.FinalMode;
                    break;

                case NativeMethods.VK_ESCAPE:
                    key = KeyboardKey.Escape;
                    break;

                case NativeMethods.VK_CONVERT:
                    key = KeyboardKey.ImeConvert;
                    break;

                case NativeMethods.VK_NONCONVERT:
                    key = KeyboardKey.ImeNonConvert;
                    break;

                case NativeMethods.VK_ACCEPT:
                    key = KeyboardKey.ImeAccept;
                    break;

                case NativeMethods.VK_MODECHANGE:
                    key = KeyboardKey.ImeModeChange;
                    break;

                case NativeMethods.VK_SPACE:
                    key = KeyboardKey.Space;
                    break;

                case NativeMethods.VK_PRIOR:
                    key = KeyboardKey.Prior;
                    break;

                case NativeMethods.VK_NEXT:
                    key = KeyboardKey.Next;
                    break;

                case NativeMethods.VK_END:
                    key = KeyboardKey.End;
                    break;

                case NativeMethods.VK_HOME:
                    key = KeyboardKey.Home;
                    break;

                case NativeMethods.VK_LEFT:
                    key = KeyboardKey.Left;
                    break;

                case NativeMethods.VK_UP:
                    key = KeyboardKey.Up;
                    break;

                case NativeMethods.VK_RIGHT:
                    key = KeyboardKey.Right;
                    break;

                case NativeMethods.VK_DOWN:
                    key = KeyboardKey.Down;
                    break;

                case NativeMethods.VK_SELECT:
                    key = KeyboardKey.Select;
                    break;

                case NativeMethods.VK_PRINT:
                    key = KeyboardKey.Print;
                    break;

                case NativeMethods.VK_EXECUTE:
                    key = KeyboardKey.Execute;
                    break;

                case NativeMethods.VK_INSERT:
                    key = KeyboardKey.Insert;
                    break;

                case NativeMethods.VK_DELETE:
                    key = KeyboardKey.Delete;
                    break;

                case NativeMethods.VK_HELP:
                    key = KeyboardKey.Help;
                    break;

                case NativeMethods.VK_0:
                    key = KeyboardKey.D0;
                    break;

                case NativeMethods.VK_1:
                    key = KeyboardKey.D1;
                    break;

                case NativeMethods.VK_2:
                    key = KeyboardKey.D2;
                    break;

                case NativeMethods.VK_3:
                    key = KeyboardKey.D3;
                    break;

                case NativeMethods.VK_4:
                    key = KeyboardKey.D4;
                    break;

                case NativeMethods.VK_5:
                    key = KeyboardKey.D5;
                    break;

                case NativeMethods.VK_6:
                    key = KeyboardKey.D6;
                    break;

                case NativeMethods.VK_7:
                    key = KeyboardKey.D7;
                    break;

                case NativeMethods.VK_8:
                    key = KeyboardKey.D8;
                    break;

                case NativeMethods.VK_9:
                    key = KeyboardKey.D9;
                    break;

                case NativeMethods.VK_A:
                    key = KeyboardKey.A;
                    break;

                case NativeMethods.VK_B:
                    key = KeyboardKey.B;
                    break;

                case NativeMethods.VK_C:
                    key = KeyboardKey.C;
                    break;

                case NativeMethods.VK_D:
                    key = KeyboardKey.D;
                    break;

                case NativeMethods.VK_E:
                    key = KeyboardKey.E;
                    break;

                case NativeMethods.VK_F:
                    key = KeyboardKey.F;
                    break;

                case NativeMethods.VK_G:
                    key = KeyboardKey.G;
                    break;

                case NativeMethods.VK_H:
                    key = KeyboardKey.H;
                    break;

                case NativeMethods.VK_I:
                    key = KeyboardKey.I;
                    break;

                case NativeMethods.VK_J:
                    key = KeyboardKey.J;
                    break;

                case NativeMethods.VK_K:
                    key = KeyboardKey.K;
                    break;

                case NativeMethods.VK_L:
                    key = KeyboardKey.L;
                    break;

                case NativeMethods.VK_M:
                    key = KeyboardKey.M;
                    break;

                case NativeMethods.VK_N:
                    key = KeyboardKey.N;
                    break;

                case NativeMethods.VK_O:
                    key = KeyboardKey.O;
                    break;

                case NativeMethods.VK_P:
                    key = KeyboardKey.P;
                    break;

                case NativeMethods.VK_Q:
                    key = KeyboardKey.Q;
                    break;

                case NativeMethods.VK_R:
                    key = KeyboardKey.R;
                    break;

                case NativeMethods.VK_S:
                    key = KeyboardKey.S;
                    break;

                case NativeMethods.VK_T:
                    key = KeyboardKey.T;
                    break;

                case NativeMethods.VK_U:
                    key = KeyboardKey.U;
                    break;

                case NativeMethods.VK_V:
                    key = KeyboardKey.V;
                    break;

                case NativeMethods.VK_W:
                    key = KeyboardKey.W;
                    break;

                case NativeMethods.VK_X:
                    key = KeyboardKey.X;
                    break;

                case NativeMethods.VK_Y:
                    key = KeyboardKey.Y;
                    break;

                case NativeMethods.VK_Z:
                    key = KeyboardKey.Z;
                    break;

                case NativeMethods.VK_LWIN:
                    key = KeyboardKey.LWin;
                    break;

                case NativeMethods.VK_RWIN:
                    key = KeyboardKey.RWin;
                    break;

                case NativeMethods.VK_APPS:
                    key = KeyboardKey.Apps;
                    break;

                case NativeMethods.VK_SLEEP:
                    key = KeyboardKey.Sleep;
                    break;

                case NativeMethods.VK_NUMPAD0:
                    key = KeyboardKey.NumPad0;
                    break;

                case NativeMethods.VK_NUMPAD1:
                    key = KeyboardKey.NumPad1;
                    break;

                case NativeMethods.VK_NUMPAD2:
                    key = KeyboardKey.NumPad2;
                    break;

                case NativeMethods.VK_NUMPAD3:
                    key = KeyboardKey.NumPad3;
                    break;

                case NativeMethods.VK_NUMPAD4:
                    key = KeyboardKey.NumPad4;
                    break;

                case NativeMethods.VK_NUMPAD5:
                    key = KeyboardKey.NumPad5;
                    break;

                case NativeMethods.VK_NUMPAD6:
                    key = KeyboardKey.NumPad6;
                    break;

                case NativeMethods.VK_NUMPAD7:
                    key = KeyboardKey.NumPad7;
                    break;

                case NativeMethods.VK_NUMPAD8:
                    key = KeyboardKey.NumPad8;
                    break;

                case NativeMethods.VK_NUMPAD9:
                    key = KeyboardKey.NumPad9;
                    break;

                case NativeMethods.VK_MULTIPLY:
                    key = KeyboardKey.Multiply;
                    break;

                case NativeMethods.VK_ADD:
                    key = KeyboardKey.Add;
                    break;

                case NativeMethods.VK_SEPARATOR:
                    key = KeyboardKey.Separator;
                    break;

                case NativeMethods.VK_SUBTRACT:
                    key = KeyboardKey.Subtract;
                    break;

                case NativeMethods.VK_DECIMAL:
                    key = KeyboardKey.Decimal;
                    break;

                case NativeMethods.VK_DIVIDE:
                    key = KeyboardKey.Divide;
                    break;

                case NativeMethods.VK_F1:
                    key = KeyboardKey.F1;
                    break;

                case NativeMethods.VK_F2:
                    key = KeyboardKey.F2;
                    break;

                case NativeMethods.VK_F3:
                    key = KeyboardKey.F3;
                    break;

                case NativeMethods.VK_F4:
                    key = KeyboardKey.F4;
                    break;

                case NativeMethods.VK_F5:
                    key = KeyboardKey.F5;
                    break;

                case NativeMethods.VK_F6:
                    key = KeyboardKey.F6;
                    break;

                case NativeMethods.VK_F7:
                    key = KeyboardKey.F7;
                    break;

                case NativeMethods.VK_F8:
                    key = KeyboardKey.F8;
                    break;

                case NativeMethods.VK_F9:
                    key = KeyboardKey.F9;
                    break;

                case NativeMethods.VK_F10:
                    key = KeyboardKey.F10;
                    break;

                case NativeMethods.VK_F11:
                    key = KeyboardKey.F11;
                    break;

                case NativeMethods.VK_F12:
                    key = KeyboardKey.F12;
                    break;

                case NativeMethods.VK_F13:
                    key = KeyboardKey.F13;
                    break;

                case NativeMethods.VK_F14:
                    key = KeyboardKey.F14;
                    break;

                case NativeMethods.VK_F15:
                    key = KeyboardKey.F15;
                    break;

                case NativeMethods.VK_F16:
                    key = KeyboardKey.F16;
                    break;

                case NativeMethods.VK_F17:
                    key = KeyboardKey.F17;
                    break;

                case NativeMethods.VK_F18:
                    key = KeyboardKey.F18;
                    break;

                case NativeMethods.VK_F19:
                    key = KeyboardKey.F19;
                    break;

                case NativeMethods.VK_F20:
                    key = KeyboardKey.F20;
                    break;

                case NativeMethods.VK_F21:
                    key = KeyboardKey.F21;
                    break;

                case NativeMethods.VK_F22:
                    key = KeyboardKey.F22;
                    break;

                case NativeMethods.VK_F23:
                    key = KeyboardKey.F23;
                    break;

                case NativeMethods.VK_F24:
                    key = KeyboardKey.F24;
                    break;

                case NativeMethods.VK_NUMLOCK:
                    key = KeyboardKey.NumLock;
                    break;

                case NativeMethods.VK_SCROLL:
                    key = KeyboardKey.Scroll;
                    break;

                case NativeMethods.VK_SHIFT:
                case NativeMethods.VK_LSHIFT:
                    key = KeyboardKey.LeftShift;
                    break;

                case NativeMethods.VK_RSHIFT:
                    key = KeyboardKey.RightShift;
                    break;

                case NativeMethods.VK_CONTROL:
                case NativeMethods.VK_LCONTROL:
                    key = KeyboardKey.LeftCtrl;
                    break;

                case NativeMethods.VK_RCONTROL:
                    key = KeyboardKey.RightCtrl;
                    break;

                case NativeMethods.VK_MENU:
                case NativeMethods.VK_LMENU:
                    key = KeyboardKey.LeftAlt;
                    break;

                case NativeMethods.VK_RMENU:
                    key = KeyboardKey.RightAlt;
                    break;

                case NativeMethods.VK_BROWSER_BACK:
                    key = KeyboardKey.BrowserBack;
                    break;

                case NativeMethods.VK_BROWSER_FORWARD:
                    key = KeyboardKey.BrowserForward;
                    break;

                case NativeMethods.VK_BROWSER_REFRESH:
                    key = KeyboardKey.BrowserRefresh;
                    break;

                case NativeMethods.VK_BROWSER_STOP:
                    key = KeyboardKey.BrowserStop;
                    break;

                case NativeMethods.VK_BROWSER_SEARCH:
                    key = KeyboardKey.BrowserSearch;
                    break;

                case NativeMethods.VK_BROWSER_FAVORITES:
                    key = KeyboardKey.BrowserFavorites;
                    break;

                case NativeMethods.VK_BROWSER_HOME:
                    key = KeyboardKey.BrowserHome;
                    break;

                case NativeMethods.VK_VOLUME_MUTE:
                    key = KeyboardKey.VolumeMute;
                    break;

                case NativeMethods.VK_VOLUME_DOWN:
                    key = KeyboardKey.VolumeDown;
                    break;

                case NativeMethods.VK_VOLUME_UP:
                    key = KeyboardKey.VolumeUp;
                    break;

                case NativeMethods.VK_MEDIA_NEXT_TRACK:
                    key = KeyboardKey.MediaNextTrack;
                    break;

                case NativeMethods.VK_MEDIA_PREV_TRACK:
                    key = KeyboardKey.MediaPreviousTrack;
                    break;

                case NativeMethods.VK_MEDIA_STOP:
                    key = KeyboardKey.MediaStop;
                    break;

                case NativeMethods.VK_MEDIA_PLAY_PAUSE:
                    key = KeyboardKey.MediaPlayPause;
                    break;

                case NativeMethods.VK_LAUNCH_MAIL:
                    key = KeyboardKey.LaunchMail;
                    break;

                case NativeMethods.VK_LAUNCH_MEDIA_SELECT:
                    key = KeyboardKey.SelectMedia;
                    break;

                case NativeMethods.VK_LAUNCH_APP1:
                    key = KeyboardKey.LaunchApplication1;
                    break;

                case NativeMethods.VK_LAUNCH_APP2:
                    key = KeyboardKey.LaunchApplication2;
                    break;

                case NativeMethods.VK_OEM_1:
                    key = KeyboardKey.OemSemicolon;
                    break;

                case NativeMethods.VK_OEM_PLUS:
                    key = KeyboardKey.OemPlus;
                    break;

                case NativeMethods.VK_OEM_COMMA:
                    key = KeyboardKey.OemComma;
                    break;

                case NativeMethods.VK_OEM_MINUS:
                    key = KeyboardKey.OemMinus;
                    break;

                case NativeMethods.VK_OEM_PERIOD:
                    key = KeyboardKey.OemPeriod;
                    break;

                case NativeMethods.VK_OEM_2:
                    key = KeyboardKey.OemQuestion;
                    break;

                case NativeMethods.VK_OEM_3:
                    key = KeyboardKey.OemTilde;
                    break;


                case NativeMethods.VK_OEM_4:
                    key = KeyboardKey.OemOpenBrackets;
                    break;

                case NativeMethods.VK_OEM_5:
                    key = KeyboardKey.OemPipe;
                    break;

                case NativeMethods.VK_OEM_6:
                    key = KeyboardKey.OemCloseBrackets;
                    break;

                case NativeMethods.VK_OEM_7:
                    key = KeyboardKey.OemQuotes;
                    break;

                case NativeMethods.VK_OEM_102:
                    key = KeyboardKey.OemBackslash;
                    break;

                case NativeMethods.VK_PROCESSKEY:
                    key = KeyboardKey.ImeProcessed;
                    break;

                case NativeMethods.VK_OEM_ATTN: // VK_DBE_ALPHANUMERIC
                    key = KeyboardKey.OemAttn;          // DbeAlphanumeric
                    break;

                case NativeMethods.VK_OEM_FINISH: // VK_DBE_KATAKANA
                    key = KeyboardKey.OemFinish;          // DbeKatakana
                    break;

                case NativeMethods.VK_OEM_COPY: // VK_DBE_HIRAGANA
                    key = KeyboardKey.OemCopy;          // DbeHiragana
                    break;

                case NativeMethods.VK_OEM_AUTO: // VK_DBE_SBCSCHAR
                    key = KeyboardKey.OemAuto;          // DbeSbcsChar
                    break;

                case NativeMethods.VK_OEM_ENLW: // VK_DBE_DBCSCHAR
                    key = KeyboardKey.OemEnlw;          // DbeDbcsChar
                    break;

                case NativeMethods.VK_OEM_BACKTAB: // VK_DBE_ROMAN
                    key = KeyboardKey.OemBackTab;          // DbeRoman
                    break;

                case NativeMethods.VK_ATTN: // VK_DBE_NOROMAN
                    key = KeyboardKey.Attn;         // DbeNoRoman
                    break;

                case NativeMethods.VK_CRSEL: // VK_DBE_ENTERWORDREGISTERMODE
                    key = KeyboardKey.CrSel;         // DbeEnterWordRegisterMode
                    break;

                case NativeMethods.VK_EXSEL: // VK_DBE_ENTERIMECONFIGMODE
                    key = KeyboardKey.ExSel;         // DbeEnterImeConfigMode
                    break;

                case NativeMethods.VK_EREOF: // VK_DBE_FLUSHSTRING
                    key = KeyboardKey.EraseEof;      // DbeFlushString
                    break;

                case NativeMethods.VK_PLAY: // VK_DBE_CODEINPUT
                    key = KeyboardKey.Play;         // DbeCodeInput
                    break;

                case NativeMethods.VK_ZOOM: // VK_DBE_NOCODEINPUT
                    key = KeyboardKey.Zoom;         // DbeNoCodeInput
                    break;

                case NativeMethods.VK_NONAME: // VK_DBE_DETERMINESTRING
                    key = KeyboardKey.NoName;         // DbeDetermineString
                    break;

                case NativeMethods.VK_PA1: // VK_DBE_ENTERDLGCONVERSIONMODE
                    key = KeyboardKey.Pa1;         // DbeEnterDlgConversionMode
                    break;

                case NativeMethods.VK_OEM_CLEAR:
                    key = KeyboardKey.OemClear;
                    break;

                default:
                    key = KeyboardKey.None;
                    break;
            }

            return key;
        }

        /// <summary>
        ///     Convert our Key enum into a Win32 VirtualKeyboardKey.
        /// </summary>
        public static int VirtualKeyFromKey(KeyboardKey key)
        {
            int virtualKey = 0;

            switch (key)
            {
                case KeyboardKey.Cancel:
                    virtualKey = NativeMethods.VK_CANCEL;
                    break;

                case KeyboardKey.Back:
                    virtualKey = NativeMethods.VK_BACK;
                    break;

                case KeyboardKey.Tab:
                    virtualKey = NativeMethods.VK_TAB;
                    break;

                case KeyboardKey.Clear:
                    virtualKey = NativeMethods.VK_CLEAR;
                    break;

                case KeyboardKey.Return:
                    virtualKey = NativeMethods.VK_RETURN;
                    break;

                case KeyboardKey.Pause:
                    virtualKey = NativeMethods.VK_PAUSE;
                    break;

                case KeyboardKey.CapsLock:
                    virtualKey = NativeMethods.VK_CAPSLOCK;
                    break;

                case KeyboardKey.JunjaMode:
                    virtualKey = NativeMethods.VK_JUNJA;
                    break;

                case KeyboardKey.FinalMode:
                    virtualKey = NativeMethods.VK_FINAL;
                    break;

                case KeyboardKey.Escape:
                    virtualKey = NativeMethods.VK_ESCAPE;
                    break;

                case KeyboardKey.ImeConvert:
                    virtualKey = NativeMethods.VK_CONVERT;
                    break;

                case KeyboardKey.ImeNonConvert:
                    virtualKey = NativeMethods.VK_NONCONVERT;
                    break;

                case KeyboardKey.ImeAccept:
                    virtualKey = NativeMethods.VK_ACCEPT;
                    break;

                case KeyboardKey.ImeModeChange:
                    virtualKey = NativeMethods.VK_MODECHANGE;
                    break;

                case KeyboardKey.Space:
                    virtualKey = NativeMethods.VK_SPACE;
                    break;

                case KeyboardKey.Prior:
                    virtualKey = NativeMethods.VK_PRIOR;
                    break;

                case KeyboardKey.Next:
                    virtualKey = NativeMethods.VK_NEXT;
                    break;

                case KeyboardKey.End:
                    virtualKey = NativeMethods.VK_END;
                    break;

                case KeyboardKey.Home:
                    virtualKey = NativeMethods.VK_HOME;
                    break;

                case KeyboardKey.Left:
                    virtualKey = NativeMethods.VK_LEFT;
                    break;

                case KeyboardKey.Up:
                    virtualKey = NativeMethods.VK_UP;
                    break;

                case KeyboardKey.Right:
                    virtualKey = NativeMethods.VK_RIGHT;
                    break;

                case KeyboardKey.Down:
                    virtualKey = NativeMethods.VK_DOWN;
                    break;

                case KeyboardKey.Select:
                    virtualKey = NativeMethods.VK_SELECT;
                    break;

                case KeyboardKey.Print:
                    virtualKey = NativeMethods.VK_PRINT;
                    break;

                case KeyboardKey.Execute:
                    virtualKey = NativeMethods.VK_EXECUTE;
                    break;
                
                case KeyboardKey.Insert:
                    virtualKey = NativeMethods.VK_INSERT;
                    break;

                case KeyboardKey.Delete:
                    virtualKey = NativeMethods.VK_DELETE;
                    break;

                case KeyboardKey.Help:
                    virtualKey = NativeMethods.VK_HELP;
                    break;

                case KeyboardKey.D0:
                    virtualKey = NativeMethods.VK_0;
                    break;

                case KeyboardKey.D1:
                    virtualKey = NativeMethods.VK_1;
                    break;

                case KeyboardKey.D2:
                    virtualKey = NativeMethods.VK_2;
                    break;

                case KeyboardKey.D3:
                    virtualKey = NativeMethods.VK_3;
                    break;

                case KeyboardKey.D4:
                    virtualKey = NativeMethods.VK_4;
                    break;

                case KeyboardKey.D5:
                    virtualKey = NativeMethods.VK_5;
                    break;

                case KeyboardKey.D6:
                    virtualKey = NativeMethods.VK_6;
                    break;

                case KeyboardKey.D7:
                    virtualKey = NativeMethods.VK_7;
                    break;

                case KeyboardKey.D8:
                    virtualKey = NativeMethods.VK_8;
                    break;

                case KeyboardKey.D9:
                    virtualKey = NativeMethods.VK_9;
                    break;

                case KeyboardKey.A:
                    virtualKey = NativeMethods.VK_A;
                    break;

                case KeyboardKey.B:
                    virtualKey = NativeMethods.VK_B;
                    break;

                case KeyboardKey.C:
                    virtualKey = NativeMethods.VK_C;
                    break;

                case KeyboardKey.D:
                    virtualKey = NativeMethods.VK_D;
                    break;

                case KeyboardKey.E:
                    virtualKey = NativeMethods.VK_E;
                    break;

                case KeyboardKey.F:
                    virtualKey = NativeMethods.VK_F;
                    break;

                case KeyboardKey.G:
                    virtualKey = NativeMethods.VK_G;
                    break;

                case KeyboardKey.H:
                    virtualKey = NativeMethods.VK_H;
                    break;

                case KeyboardKey.I:
                    virtualKey = NativeMethods.VK_I;
                    break;

                case KeyboardKey.J:
                    virtualKey = NativeMethods.VK_J;
                    break;

                case KeyboardKey.K:
                    virtualKey = NativeMethods.VK_K;
                    break;

                case KeyboardKey.L:
                    virtualKey = NativeMethods.VK_L;
                    break;

                case KeyboardKey.M:
                    virtualKey = NativeMethods.VK_M;
                    break;

                case KeyboardKey.N:
                    virtualKey = NativeMethods.VK_N;
                    break;

                case KeyboardKey.O:
                    virtualKey = NativeMethods.VK_O;
                    break;

                case KeyboardKey.P:
                    virtualKey = NativeMethods.VK_P;
                    break;

                case KeyboardKey.Q:
                    virtualKey = NativeMethods.VK_Q;
                    break;

                case KeyboardKey.R:
                    virtualKey = NativeMethods.VK_R;
                    break;

                case KeyboardKey.S:
                    virtualKey = NativeMethods.VK_S;
                    break;

                case KeyboardKey.T:
                    virtualKey = NativeMethods.VK_T;
                    break;

                case KeyboardKey.U:
                    virtualKey = NativeMethods.VK_U;
                    break;

                case KeyboardKey.V:
                    virtualKey = NativeMethods.VK_V;
                    break;

                case KeyboardKey.W:
                    virtualKey = NativeMethods.VK_W;
                    break;

                case KeyboardKey.X:
                    virtualKey = NativeMethods.VK_X;
                    break;

                case KeyboardKey.Y:
                    virtualKey = NativeMethods.VK_Y;
                    break;

                case KeyboardKey.Z:
                    virtualKey = NativeMethods.VK_Z;
                    break;

                case KeyboardKey.LWin:
                    virtualKey = NativeMethods.VK_LWIN;
                    break;

                case KeyboardKey.RWin:
                    virtualKey = NativeMethods.VK_RWIN;
                    break;

                case KeyboardKey.Apps:
                    virtualKey = NativeMethods.VK_APPS;
                    break;

                case KeyboardKey.Sleep:
                    virtualKey = NativeMethods.VK_SLEEP;
                    break;

                case KeyboardKey.NumPad0:
                    virtualKey = NativeMethods.VK_NUMPAD0;
                    break;

                case KeyboardKey.NumPad1:
                    virtualKey = NativeMethods.VK_NUMPAD1;
                    break;

                case KeyboardKey.NumPad2:
                    virtualKey = NativeMethods.VK_NUMPAD2;
                    break;

                case KeyboardKey.NumPad3:
                    virtualKey = NativeMethods.VK_NUMPAD3;
                    break;

                case KeyboardKey.NumPad4:
                    virtualKey = NativeMethods.VK_NUMPAD4;
                    break;

                case KeyboardKey.NumPad5:
                    virtualKey = NativeMethods.VK_NUMPAD5;
                    break;

                case KeyboardKey.NumPad6:
                    virtualKey = NativeMethods.VK_NUMPAD6;
                    break;

                case KeyboardKey.NumPad7:
                    virtualKey = NativeMethods.VK_NUMPAD7;
                    break;

                case KeyboardKey.NumPad8:
                    virtualKey = NativeMethods.VK_NUMPAD8;
                    break;

                case KeyboardKey.NumPad9:
                    virtualKey = NativeMethods.VK_NUMPAD9;
                    break;

                case KeyboardKey.Multiply:
                    virtualKey = NativeMethods.VK_MULTIPLY;
                    break;

                case KeyboardKey.Add:
                    virtualKey = NativeMethods.VK_ADD;
                    break;

                case KeyboardKey.Separator:
                    virtualKey = NativeMethods.VK_SEPARATOR;
                    break;

                case KeyboardKey.Subtract:
                    virtualKey = NativeMethods.VK_SUBTRACT;
                    break;

                case KeyboardKey.Decimal:
                    virtualKey = NativeMethods.VK_DECIMAL;
                    break;

                case KeyboardKey.Divide:
                    virtualKey = NativeMethods.VK_DIVIDE;
                    break;

                case KeyboardKey.F1:
                    virtualKey = NativeMethods.VK_F1;
                    break;

                case KeyboardKey.F2:
                    virtualKey = NativeMethods.VK_F2;
                    break;

                case KeyboardKey.F3:
                    virtualKey = NativeMethods.VK_F3;
                    break;

                case KeyboardKey.F4:
                    virtualKey = NativeMethods.VK_F4;
                    break;

                case KeyboardKey.F5:
                    virtualKey = NativeMethods.VK_F5;
                    break;

                case KeyboardKey.F6:
                    virtualKey = NativeMethods.VK_F6;
                    break;

                case KeyboardKey.F7:
                    virtualKey = NativeMethods.VK_F7;
                    break;

                case KeyboardKey.F8:
                    virtualKey = NativeMethods.VK_F8;
                    break;

                case KeyboardKey.F9:
                    virtualKey = NativeMethods.VK_F9;
                    break;

                case KeyboardKey.F10:
                    virtualKey = NativeMethods.VK_F10;
                    break;

                case KeyboardKey.F11:
                    virtualKey = NativeMethods.VK_F11;
                    break;

                case KeyboardKey.F12:
                    virtualKey = NativeMethods.VK_F12;
                    break;

                case KeyboardKey.F13:
                    virtualKey = NativeMethods.VK_F13;
                    break;

                case KeyboardKey.F14:
                    virtualKey = NativeMethods.VK_F14;
                    break;

                case KeyboardKey.F15:
                    virtualKey = NativeMethods.VK_F15;
                    break;

                case KeyboardKey.F16:
                    virtualKey = NativeMethods.VK_F16;
                    break;

                case KeyboardKey.F17:
                    virtualKey = NativeMethods.VK_F17;
                    break;

                case KeyboardKey.F18:
                    virtualKey = NativeMethods.VK_F18;
                    break;

                case KeyboardKey.F19:
                    virtualKey = NativeMethods.VK_F19;
                    break;

                case KeyboardKey.F20:
                    virtualKey = NativeMethods.VK_F20;
                    break;

                case KeyboardKey.F21:
                    virtualKey = NativeMethods.VK_F21;
                    break;

                case KeyboardKey.F22:
                    virtualKey = NativeMethods.VK_F22;
                    break;

                case KeyboardKey.F23:
                    virtualKey = NativeMethods.VK_F23;
                    break;

                case KeyboardKey.F24:
                    virtualKey = NativeMethods.VK_F24;
                    break;

                case KeyboardKey.NumLock:
                    virtualKey = NativeMethods.VK_NUMLOCK;
                    break;

                case KeyboardKey.Scroll:
                    virtualKey = NativeMethods.VK_SCROLL;
                    break;

                case KeyboardKey.LeftShift:
                    virtualKey = NativeMethods.VK_LSHIFT;
                    break;

                case KeyboardKey.RightShift:
                    virtualKey = NativeMethods.VK_RSHIFT;
                    break;

                case KeyboardKey.LeftCtrl:
                    virtualKey = NativeMethods.VK_LCONTROL;
                    break;

                case KeyboardKey.RightCtrl:
                    virtualKey = NativeMethods.VK_RCONTROL;
                    break;

                case KeyboardKey.LeftAlt:
                    virtualKey = NativeMethods.VK_LMENU;
                    break;

                case KeyboardKey.RightAlt:
                    virtualKey = NativeMethods.VK_RMENU;
                    break;

                case KeyboardKey.BrowserBack:
                    virtualKey = NativeMethods.VK_BROWSER_BACK;
                    break;

                case KeyboardKey.BrowserForward:
                    virtualKey = NativeMethods.VK_BROWSER_FORWARD;
                    break;

                case KeyboardKey.BrowserRefresh:
                    virtualKey = NativeMethods.VK_BROWSER_REFRESH;
                    break;

                case KeyboardKey.BrowserStop:
                    virtualKey = NativeMethods.VK_BROWSER_STOP;
                    break;

                case KeyboardKey.BrowserSearch:
                    virtualKey = NativeMethods.VK_BROWSER_SEARCH;
                    break;

                case KeyboardKey.BrowserFavorites:
                    virtualKey = NativeMethods.VK_BROWSER_FAVORITES;
                    break;

                case KeyboardKey.BrowserHome:
                    virtualKey = NativeMethods.VK_BROWSER_HOME;
                    break;

                case KeyboardKey.VolumeMute:
                    virtualKey = NativeMethods.VK_VOLUME_MUTE;
                    break;

                case KeyboardKey.VolumeDown:
                    virtualKey = NativeMethods.VK_VOLUME_DOWN;
                    break;

                case KeyboardKey.VolumeUp:
                    virtualKey = NativeMethods.VK_VOLUME_UP;
                    break;

                case KeyboardKey.MediaNextTrack:
                    virtualKey = NativeMethods.VK_MEDIA_NEXT_TRACK;
                    break;

                case KeyboardKey.MediaPreviousTrack:
                    virtualKey = NativeMethods.VK_MEDIA_PREV_TRACK;
                    break;

                case KeyboardKey.MediaStop:
                    virtualKey = NativeMethods.VK_MEDIA_STOP;
                    break;

                case KeyboardKey.MediaPlayPause:
                    virtualKey = NativeMethods.VK_MEDIA_PLAY_PAUSE;
                    break;

                case KeyboardKey.LaunchMail:
                    virtualKey = NativeMethods.VK_LAUNCH_MAIL;
                    break;

                case KeyboardKey.SelectMedia:
                    virtualKey = NativeMethods.VK_LAUNCH_MEDIA_SELECT;
                    break;

                case KeyboardKey.LaunchApplication1:
                    virtualKey = NativeMethods.VK_LAUNCH_APP1;
                    break;

                case KeyboardKey.LaunchApplication2:
                    virtualKey = NativeMethods.VK_LAUNCH_APP2;
                    break;

                case KeyboardKey.OemSemicolon:
                    virtualKey = NativeMethods.VK_OEM_1;
                    break;

                case KeyboardKey.OemPlus:
                    virtualKey = NativeMethods.VK_OEM_PLUS;
                    break;

                case KeyboardKey.OemComma:
                    virtualKey = NativeMethods.VK_OEM_COMMA;
                    break;

                case KeyboardKey.OemMinus:
                    virtualKey = NativeMethods.VK_OEM_MINUS;
                    break;

                case KeyboardKey.OemPeriod:
                    virtualKey = NativeMethods.VK_OEM_PERIOD;
                    break;

                case KeyboardKey.OemQuestion:
                    virtualKey = NativeMethods.VK_OEM_2;
                    break;

                case KeyboardKey.OemTilde:
                    virtualKey = NativeMethods.VK_OEM_3;
                    break;

                case KeyboardKey.OemOpenBrackets:
                    virtualKey = NativeMethods.VK_OEM_4;
                    break;

                case KeyboardKey.OemPipe:
                    virtualKey = NativeMethods.VK_OEM_5;
                    break;

                case KeyboardKey.OemCloseBrackets:
                    virtualKey = NativeMethods.VK_OEM_6;
                    break;

                case KeyboardKey.OemQuotes:
                    virtualKey = NativeMethods.VK_OEM_7;
                    break;

                case KeyboardKey.OemBackslash:
                    virtualKey = NativeMethods.VK_OEM_102;
                    break;

                case KeyboardKey.ImeProcessed:
                    virtualKey = NativeMethods.VK_PROCESSKEY;
                    break;

                case KeyboardKey.OemAttn:                           // DbeAlphanumeric
                    virtualKey = NativeMethods.VK_OEM_ATTN; // VK_DBE_ALPHANUMERIC
                    break;

                case KeyboardKey.OemFinish:                           // DbeKatakana
                    virtualKey = NativeMethods.VK_OEM_FINISH; // VK_DBE_KATAKANA
                    break;

                case KeyboardKey.OemCopy:                           // DbeHiragana
                    virtualKey = NativeMethods.VK_OEM_COPY; // VK_DBE_HIRAGANA
                    break;

                case KeyboardKey.OemAuto:                           // DbeSbcsChar
                    virtualKey = NativeMethods.VK_OEM_AUTO; // VK_DBE_SBCSCHAR
                    break;

                case KeyboardKey.OemEnlw:                           // DbeDbcsChar
                    virtualKey = NativeMethods.VK_OEM_ENLW; // VK_DBE_DBCSCHAR
                    break;

                case KeyboardKey.OemBackTab:                           // DbeRoman
                    virtualKey = NativeMethods.VK_OEM_BACKTAB; // VK_DBE_ROMAN
                    break;

                case KeyboardKey.Attn:                          // DbeNoRoman
                    virtualKey = NativeMethods.VK_ATTN; // VK_DBE_NOROMAN
                    break;

                case KeyboardKey.CrSel:                          // DbeEnterWordRegisterMode
                    virtualKey = NativeMethods.VK_CRSEL; // VK_DBE_ENTERWORDREGISTERMODE
                    break;

                case KeyboardKey.ExSel:                          // EnterImeConfigureMode
                    virtualKey = NativeMethods.VK_EXSEL; // VK_DBE_ENTERIMECONFIGMODE
                    break;

                case KeyboardKey.EraseEof:                       // DbeFlushString
                    virtualKey = NativeMethods.VK_EREOF; // VK_DBE_FLUSHSTRING
                    break;

                case KeyboardKey.Play:                           // DbeCodeInput
                    virtualKey = NativeMethods.VK_PLAY;  // VK_DBE_CODEINPUT
                    break;

                case KeyboardKey.Zoom:                           // DbeNoCodeInput
                    virtualKey = NativeMethods.VK_ZOOM;  // VK_DBE_NOCODEINPUT
                    break;

                case KeyboardKey.NoName:                          // DbeDetermineString
                    virtualKey = NativeMethods.VK_NONAME; // VK_DBE_DETERMINESTRING
                    break;

                case KeyboardKey.Pa1:                          // DbeEnterDlgConversionMode
                    virtualKey = NativeMethods.VK_PA1; // VK_ENTERDLGCONVERSIONMODE
                    break;

                case KeyboardKey.OemClear:
                    virtualKey = NativeMethods.VK_OEM_CLEAR;
                    break;

                case KeyboardKey.DeadCharProcessed:             //This is usused.  It's just here for completeness.
                    virtualKey = 0;                     //There is no Win32 VKey for this.
                    break;

                default:
                    virtualKey = 0;
                    break;
            }

            return virtualKey;
        }
    }
}

