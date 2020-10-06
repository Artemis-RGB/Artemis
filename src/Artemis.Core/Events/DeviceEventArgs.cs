using System;
using RGB.NET.Core;

namespace Artemis.Core
{
    /// <summary>
    ///     Provides data about device related events
    /// </summary>
    public class DeviceEventArgs : EventArgs
    {
        internal DeviceEventArgs(IRGBDevice device)
        {
            Device = device;
        }

        public IRGBDevice Device { get; }
    }
}