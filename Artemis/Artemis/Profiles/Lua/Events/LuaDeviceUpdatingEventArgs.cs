using System;
using Artemis.Models.Interfaces;
using MoonSharp.Interpreter;

namespace Artemis.Profiles.Lua.Events
{
    [MoonSharpUserData]
    public class LuaDeviceUpdatingEventArgs : EventArgs
    {
        public LuaDeviceUpdatingEventArgs(string deviceType, IDataModel dataModel, bool preview)
        {
            DeviceType = deviceType;
            DataModel = dataModel;
            Preview = preview;
        }

        public string DeviceType { get; set; }
        public IDataModel DataModel { get; }
        public bool Preview { get; }
    }
}