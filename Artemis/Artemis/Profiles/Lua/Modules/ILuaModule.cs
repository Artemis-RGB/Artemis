using System;

namespace Artemis.Profiles.Lua.Modules
{
    public interface ILuaModule : IDisposable
    {
        bool AlwaysPresent { get; }
        string ModuleName { get; }
    }
}
