using System;
using Artemis.Core;

namespace Artemis.UI.Avalonia.Shared.Events
{
    /// <summary>
    ///     Provides data on LED click events raised by the device visualizer
    /// </summary>
    public class LedClickedEventArgs : EventArgs
    {
        internal LedClickedEventArgs(ArtemisDevice device, ArtemisLed led)
        {
            Device = device;
            Led = led;
        }

        /// <summary>
        ///     The device that was clicked
        /// </summary>
        public ArtemisDevice Device { get; set; }

        /// <summary>
        ///     The LED that was clicked
        /// </summary>
        public ArtemisLed Led { get; set; }
    }
}