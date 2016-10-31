using System;
using Artemis.Models.Interfaces;
using MoonSharp.Interpreter;

namespace Artemis.Profiles.Lua.Events
{
    [MoonSharpUserData]
    public class LuaProfileUpdatingEventArgs : EventArgs
    {
        public LuaProfileUpdatingEventArgs(IDataModel dataModel, bool preview)
        {
            DataModel = dataModel;
            Preview = preview;
        }

        public IDataModel DataModel { get; }
        public bool Preview { get; }
    }
}