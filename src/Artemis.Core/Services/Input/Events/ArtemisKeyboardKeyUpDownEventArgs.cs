namespace Artemis.Core.Services
{
    /// <summary>
    ///     Contains data for keyboard input events
    /// </summary>
    public class ArtemisKeyboardKeyUpDownEventArgs : ArtemisKeyboardKeyEventArgs
    {
        internal ArtemisKeyboardKeyUpDownEventArgs(ArtemisDevice? device, ArtemisLed? led, KeyboardKey key, KeyboardModifierKey modifiers, bool isDown) : base(device, led, key, modifiers)
        {
            IsDown = isDown;
        }

        /// <summary>
        /// Whether the key is being pressed down, if <see langword="false"/> the key is being released
        /// </summary>
        public bool IsDown { get; set; }
    }
}