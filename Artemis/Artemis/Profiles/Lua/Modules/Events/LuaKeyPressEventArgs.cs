using System;
using System.Windows.Forms;
using MoonSharp.Interpreter;

namespace Artemis.Profiles.Lua.Modules.Events
{
    [MoonSharpUserData]
    public class LuaKeyPressEventArgs : EventArgs
    {
        private readonly Keys _key;

        public LuaKeyPressEventArgs(Keys key, int x, int y)
        {
            _key = key;
            X = x;
            Y = y;
        }

        public string Key => _key.ToString();
        public int X { get; }
        public int Y { get; }
    }
}