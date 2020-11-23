using System;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Contains data for mouse input events
    /// </summary>
    public class MouseButtonEventArgs : EventArgs
    {
        internal MouseButtonEventArgs(ArtemisDevice? device, ArtemisLed? led, MouseButton button)
        {
            Device = device;
            Led = led;
            Button = button;
        }

        /// <summary>
        ///     Gets the device that triggered the event
        /// </summary>
        public ArtemisDevice? Device { get; }

        /// <summary>
        /// Gets the LED that corresponds to the pressed key
        /// </summary>
        public ArtemisLed? Led { get; }

        /// <summary>
        ///     Gets the button that triggered the event
        /// </summary>
        public MouseButton Button { get; }
    }
}