using System;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Contains data for input provider keyboard events
    /// </summary>
    public class InputProviderKeyboardEventArgs : EventArgs
    {
        /// <summary>
        /// </summary>
        /// <param name="device">The device that triggered the event</param>
        /// <param name="key">The key that triggered the event</param>
        /// <param name="isDown">Whether the key is pressed down</param>
        public InputProviderKeyboardEventArgs(ArtemisDevice? device, KeyboardKey key, bool isDown)
        {
            Device = device;
            Key = key;
            IsDown = isDown;
        }

        /// <summary>
        ///     Gets the device that triggered the event
        /// </summary>
        public ArtemisDevice? Device { get; }

        /// <summary>
        ///     Gets the key that triggered the event
        /// </summary>
        public KeyboardKey Key { get; }

        /// <summary>
        ///     Gets whether the key is pressed down
        /// </summary>
        public bool IsDown { get; }
    }
}