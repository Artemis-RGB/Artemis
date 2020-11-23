using System;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Contains data for input provider mouse button events
    /// </summary>
    public class InputProviderMouseScrollEventArgs : EventArgs
    {
        /// <summary>
        /// </summary>
        /// <param name="device">The device that triggered the event</param>
        /// <param name="direction">The direction in which was scrolled</param>
        /// <param name="delta">The scroll delta (can positive or negative)</param>
        public InputProviderMouseScrollEventArgs(ArtemisDevice? device, MouseScrollDirection direction, int delta)
        {
            Device = device;
            Direction = direction;
            Delta = delta;
        }

        /// <summary>
        ///     Gets the device that triggered the event
        /// </summary>
        public ArtemisDevice? Device { get; }

        /// <summary>
        ///     Gets the direction in which was scrolled
        /// </summary>
        public MouseScrollDirection Direction { get; }

        /// <summary>
        ///     Gets the scroll delta (can positive or negative)
        /// </summary>
        public int Delta { get; }
    }
}