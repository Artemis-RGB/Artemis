using System;

namespace Artemis.Core
{
    /// <summary>
    ///     Provides data about device related events
    /// </summary>
    public class DeviceEventArgs : EventArgs
    {
        internal DeviceEventArgs(ArtemisDevice device)
        {
            Device = device;
        }

        /// <summary>
        ///     Gets the device this event is related to
        /// </summary>
        public ArtemisDevice Device { get; }
    }
}