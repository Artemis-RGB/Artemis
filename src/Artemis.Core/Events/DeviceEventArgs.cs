using System;
using RGB.NET.Core;

namespace Artemis.Core.Events
{
    public class DeviceEventArgs : EventArgs
    {
        public DeviceEventArgs()
        {
        }

        public DeviceEventArgs(IRGBDevice device)
        {
            Device = device;
        }

        public IRGBDevice Device { get; }
    }
}