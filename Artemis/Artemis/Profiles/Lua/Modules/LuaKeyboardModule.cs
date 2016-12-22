using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Artemis.DeviceProviders;
using MoonSharp.Interpreter.Interop;

namespace Artemis.Profiles.Lua.Modules
{
    public class LuaKeyboardModule : ILuaModule
    {
        public bool AlwaysPresent => true;
        public string ModuleName => "Keyboard";

        private readonly KeyboardProvider _keyboardProvider;

        public LuaKeyboardModule(KeyboardProvider keyboardProvider)
        {
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
                LuaWrapper.LuaEventsWrapper.InvokeKeyPressed(LuaWrapper.ProfileModel, this, keyMatch.Value.KeyCode,
                    keyMatch.Value.X, keyMatch.Value.Y);
        }

        public void PressKeys(string keys)
        {
            SendKeys.SendWait(keys);
        }

        public void GetKeyPosition(Keys key)
        {
            _keyboardProvider.GetKeyPosition(key);
        }
    }
}
