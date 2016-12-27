using System.Collections.Generic;
using Artemis.Managers;
using Artemis.Profiles.Lua.Modules.Timer;
using MoonSharp.Interpreter;

namespace Artemis.Profiles.Lua.Modules
{
    [MoonSharpUserData]
    public class LuaTimerModule : LuaModule
    {
        private readonly List<LuaTimer> _timers;

        public LuaTimerModule(LuaManager luaManager) : base(luaManager)
        {
            _timers = new List<LuaTimer>();
        }

        public override string ModuleName => "Timer";

        public override void Dispose()
        {
            foreach (var luaTimer in _timers)
                luaTimer.Dispose();

            _timers.Clear();
        }

        public LuaTimer SetTimer(DynValue function, int interval, int timesToExecute, params DynValue[] args)
        {
            var luaTimer = new LuaTimer(this, function, interval, timesToExecute, args);
            _timers.Add(luaTimer);
            return luaTimer;
        }

        public void RemoveTimer(LuaTimer luaTimer)
        {
            if (_timers.Contains(luaTimer))
                _timers.Remove(luaTimer);
            
            luaTimer.Dispose();
        }
    }
}