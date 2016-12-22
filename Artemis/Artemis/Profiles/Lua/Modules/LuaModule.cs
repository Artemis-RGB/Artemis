using System;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Artemis.Profiles.Lua.Modules
{
    /// <summary>
    ///     Defines a module which will be accessable in all LUA scripts.
    /// </summary>
    [MoonSharpUserData]
    public abstract class LuaModule : IDisposable
    {
        public LuaModule(LuaWrapper luaWrapper)
        {
            LuaWrapper = luaWrapper;
        }

        /// <summary>
        ///     The name under which this module will be accessable in LUA
        /// </summary>
        [MoonSharpVisible(false)]
        public abstract string ModuleName { get; }

        [MoonSharpVisible(false)]
        public LuaWrapper LuaWrapper { get; set; }

        [MoonSharpVisible(false)]
        public abstract void Dispose();
    }
}