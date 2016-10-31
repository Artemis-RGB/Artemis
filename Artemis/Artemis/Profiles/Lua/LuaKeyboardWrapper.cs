using System;
using System.Windows.Forms;
using Artemis.DeviceProviders;
using Artemis.Utilities.Keyboard;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Artemis.Profiles.Lua
{
    [MoonSharpUserData]
    public class LuaKeyboardWrapper : IDisposable
    {
        private readonly KeyboardProvider _keyboardProvider;
        private readonly LuaWrapper _luaWrapper;

        public LuaKeyboardWrapper(LuaWrapper luaWrapper, KeyboardProvider keyboardProvider)
        {
            _luaWrapper = luaWrapper;
            _keyboardProvider = keyboardProvider;

            KeyboardHook.KeyDownCallback += KeyboardHookOnKeyDownCallback;
        }

        public string Name => _keyboardProvider.Name;
        public string Slug => _keyboardProvider.Slug;
        public int Width => _keyboardProvider.Width;
        public int Height => _keyboardProvider.Height;

        [MoonSharpVisible(false)]
        public void Dispose()
        {
            KeyboardHook.KeyDownCallback -= KeyboardHookOnKeyDownCallback;
        }

        private void KeyboardHookOnKeyDownCallback(KeyEventArgs e)
        {
            var keyMatch = _keyboardProvider.GetKeyPosition(e.KeyCode);
            if (keyMatch != null)
                _luaWrapper.LuaEventsWrapper.InvokeKeyPressed(_luaWrapper.ProfileModel, this,
                    keyMatch.Value.KeyCode, keyMatch.Value.X, keyMatch.Value.Y);
        }

        public void SendKeys(string keys)
        {
            System.Windows.Forms.SendKeys.SendWait(keys);
        }
    }
}