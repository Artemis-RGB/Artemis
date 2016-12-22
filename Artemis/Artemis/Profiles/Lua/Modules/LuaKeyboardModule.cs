using System.Windows.Forms;
using Artemis.DeviceProviders;
using Artemis.Utilities.Keyboard;
using MoonSharp.Interpreter;

namespace Artemis.Profiles.Lua.Modules
{
    [MoonSharpUserData]
    public class LuaKeyboardModule : LuaModule
    {
        private readonly KeyboardProvider _keyboardProvider;

        public LuaKeyboardModule(LuaWrapper luaWrapper) : base(luaWrapper)
        {
            _keyboardProvider = luaWrapper.KeyboardProvider;
            KeyboardHook.KeyDownCallback += KeyboardHookOnKeyDownCallback;
        }

        // TODO: Visible in LUA? Decladed as invisile in base class
        public override string ModuleName => "Keyboard";

        public string Name => _keyboardProvider.Name;
        public string Slug => _keyboardProvider.Slug;
        public int Width => _keyboardProvider.Width;
        public int Height => _keyboardProvider.Height;

        #region Overriding members

        public override void Dispose()
        {
            KeyboardHook.KeyDownCallback -= KeyboardHookOnKeyDownCallback;
        }

        #endregion

        private void KeyboardHookOnKeyDownCallback(KeyEventArgs e)
        {
            // TODO
            //var keyMatch = _keyboardProvider.GetKeyPosition(e.KeyCode);
            //if (keyMatch != null)
            //    LuaWrapper.LuaEventsWrapper.InvokeKeyPressed(LuaWrapper.ProfileModel, this, keyMatch.Value.KeyCode,
            //        keyMatch.Value.X, keyMatch.Value.Y);
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