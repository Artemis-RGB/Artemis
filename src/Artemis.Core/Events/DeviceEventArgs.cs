using System;
using RGB.NET.Core;

namespace Artemis.Core
{
    public class DeviceEventArgs : EventArgs
    {
        public DeviceEventArgs(IRGBDevice device)
        {
            Device = device;
        }

        public IRGBDevice Device { get; }
    }
}