using System;
using System.Diagnostics;
using System.Windows.Forms;
using Artemis.DeviceProviders;
using Artemis.Managers;
using MoonSharp.Interpreter;

namespace Artemis.Profiles.Lua.Modules
{
    [MoonSharpUserData]
    public class LuaKeyboardModule : LuaModule
    {
        private readonly KeyboardProvider _keyboardProvider;

        public LuaKeyboardModule(LuaManager luaManager) : base(luaManager)
        {
            _keyboardProvider = luaManager.KeyboardProvider;

            // Register the KeyMatch type for usage in GetKeyPosition
            UserData.RegisterType(typeof(KeyMatch));
        }

        public override string ModuleName => "Keyboard";

        public string Name => _keyboardProvider.Name;
        public string Slug => _keyboardProvider.Slug;
        public int Width => _keyboardProvider.Width;
        public int Height => _keyboardProvider.Height;

        public void PressKeys(string keys)
        {
            SendKeys.SendWait(keys);
        }

        public KeyMatch? GetKeyPosition(string key)
        {
            // Convert string to Keys enum, I'm not sure if built-in enums can be converted automatically
            try
            {
                var keyEnum = (Keys)Enum.Parse(typeof(Keys), key);
                return _keyboardProvider.GetKeyPosition(keyEnum);
            }
            catch (ArgumentException)
            {
                throw new ScriptRuntimeException($"Key '{key}' not found");
            }
        }
    }
}