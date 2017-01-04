using System;
using Artemis.Modules.Abstract;
using MoonSharp.Interpreter;

namespace Artemis.Profiles.Lua.Modules.Events
{
    [MoonSharpUserData]
    public class LuaDeviceUpdatingEventArgs : EventArgs
    {
        public LuaDeviceUpdatingEventArgs(string deviceType, ModuleDataModel dataModel, bool preview)
        {
            DeviceType = deviceType;
            DataModel = dataModel;
            Preview = preview;
        }

        public string DeviceType { get; set; }
        public ModuleDataModel DataModel { get; }
        public bool Preview { get; }
    }
}