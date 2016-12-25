using System;
using Artemis.Managers;
using MoonSharp.Interpreter.Interop;

namespace Artemis.Profiles.Lua
{
    /// <summary>
    ///     Defines a module which will be accessable in all LUA scripts.
    /// </summary>
    public abstract class LuaModule : IDisposable
    {
        public LuaModule(LuaManager luaManager)
        {
            LuaManager = luaManager;
        }

        /// <summary>
        ///     The name under which this module will be accessable in LUA
        /// </summary>
        public abstract string ModuleName { get; }

        /// <summary>
        ///     The LUA manager containing this module
        /// </summary>
        [MoonSharpVisible(false)]
        public LuaManager LuaManager { get; set; }

        /// <summary>
        ///     Called when the LUA script is restarted
        /// </summary>
        [MoonSharpVisible(false)]
        public virtual void Dispose()
        {
        }
    }
}