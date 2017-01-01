using System;
using System.Timers;
using MoonSharp.Interpreter;

namespace Artemis.Profiles.Lua.Modules.Timer
{
    [MoonSharpUserData]
    public class LuaTimer : IDisposable
    {
        private readonly DynValue[] _args;
        private readonly DynValue _function;
        private readonly System.Timers.Timer _timer;
        private readonly LuaTimerModule _timerModule;
        private readonly int _timesToExecute;
        private int _timesExecuted;

        public LuaTimer(LuaTimerModule timerModule, DynValue function, int interval, int timesToExecute,
            params DynValue[] args)
        {
            _timerModule = timerModule;
            _function = function;
            _timesToExecute = timesToExecute;
            _args = args;
            _timesExecuted = 0;

            // Setup timer
            _timer = new System.Timers.Timer(interval);
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        public void Dispose()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }

        public void Stop()
        {
            _timerModule.RemoveTimer(this);
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            if (_args != null)
                _timerModule.LuaManager.Call(_function, _args);
            else
                _timerModule.LuaManager.Call(_function);

            // Don't keep track of execution if times is set to 0 (infinite)
            if (_timesToExecute <= 0)
                return;

            _timesExecuted++;
            if (_timesExecuted >= _timesToExecute)
                _timerModule.RemoveTimer(this);
        }
    }
}