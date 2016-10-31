using System;
using Artemis.Models.Interfaces;
using MoonSharp.Interpreter;

namespace Artemis.Profiles.Lua.Events
{
    [MoonSharpUserData]
    public class LuaProfileDrawingEventArgs : EventArgs
    {
        public LuaProfileDrawingEventArgs(IDataModel dataModel, bool preview, LuaDrawWrapper luaDrawWrapper)
        {
            DataModel = dataModel;
            Preview = preview;
            Drawing = luaDrawWrapper;
        }

        public IDataModel DataModel { get; }
        public bool Preview { get; }
        public LuaDrawWrapper Drawing { get; set; }
    }
}