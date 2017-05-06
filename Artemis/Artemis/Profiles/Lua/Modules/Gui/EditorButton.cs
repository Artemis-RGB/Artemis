using Artemis.Managers;
using MoonSharp.Interpreter;

namespace Artemis.Profiles.Lua.Modules.Gui
{
    public class EditorButton
    {
        private readonly LuaManager _luaManager;

        public EditorButton(LuaManager luaManager, string text, DynValue action)
        {
            _luaManager = luaManager;
            Text = text;
            Action = action;
        }

        public void Invoke()
        {
            _luaManager.Call(Action);
        }

        public string Text { get; }
        public DynValue Action { get; }
    }
}
