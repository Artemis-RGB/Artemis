using Artemis.Utilities;
using MoonSharp.Interpreter;

namespace Artemis.Profiles.Lua
{
    [MoonSharpUserData]
    public class LuaActiveWindowWrapper
    {
        public string ProcessName => ActiveWindowHelper.ActiveWindowProcessName;
        public string WindowTitle => ActiveWindowHelper.ActiveWindowWindowTitle;
    }
}
