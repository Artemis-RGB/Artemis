using System;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Contains data for keyboard input events
    /// </summary>
    public class KeyboardEventArgs : EventArgs
    {
        internal KeyboardEventArgs(ArtemisDevice? device, ArtemisLed? led, KeyboardKey key, KeyboardModifierKeys modifiers)
        {
            Device = device;
            Led = led;
            Key = key;
            Modifiers = modifiers;
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
        ///     Gets the key that triggered the event
        /// </summary>
        public KeyboardKey Key { get; }

        /// <summary>
        ///     Gets the modifiers that are pressed
        /// </summary>
        public KeyboardModifierKeys Modifiers { get; }
    }

    /// <summary>
    ///     Contains data for keyboard input events
    /// </summary>
    public class KeyboardKeyUpDownEventArgs : KeyboardEventArgs
    {
        internal KeyboardKeyUpDownEventArgs(ArtemisDevice? device, ArtemisLed? led, KeyboardKey key, KeyboardModifierKeys modifiers, bool isDown) : base(device, led, key, modifiers)
        {
            IsDown = isDown;
        }

        /// <summary>
        /// Whether the key is being pressed down, if <see langword="false"/> the key is being released
        /// </summary>
        public bool IsDown { get; set; }
    }
}