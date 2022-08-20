namespace Artemis.UI.Linux.Providers.Input
{
    /// <summary>
    /// https://www.kernel.org/doc/Documentation/input/event-codes.txt
    /// </summary>
    internal enum LinuxInputEventType : ushort
    {
        /// <summary>
        /// Used as markers to separate events. Events may be separated in time or in space, such as with the multitouch protocol.
        /// </summary>
        SYN = 0x00,

        /// <summary>
        /// Used to describe state changes of keyboards, buttons, or other key-like devices.
        /// </summary>
        KEY = 0x01,

        /// <summary>
        /// Used to describe relative axis value changes, e.g. moving the mouse 5 units to the left.
        /// </summary>
        REL = 0x02,

        /// <summary>
        /// Used to describe absolute axis value changes, e.g. describing the coordinates of a touch on a touchscreen.
        /// </summary>
        ABS = 0x03,

        /// <summary>
        /// Used to describe miscellaneous input data that do not fit into other types.
        /// </summary>
        MSC = 0x04,

        /// <summary>
        /// Used to describe binary state input switches.
        /// </summary>
        SW = 0x05,

        /// <summary>
        /// Used to turn LEDs on devices on and off.
        /// </summary>
        LED = 0x11,

        /// <summary>
        /// Used to output sound to devices.
        /// </summary>
        SND = 0x12,

        /// <summary>
        /// Used for autorepeating devices.
        /// </summary>
        REP = 0x14,

        /// <summary>
        /// Used to send force feedback commands to an input device.
        /// </summary>
        FF = 0x15,

        /// <summary>
        /// A special type for power button and switch input.
        /// </summary>
        PWR = 0x16,

        /// <summary>
        /// Used to receive force feedback device status.
        /// </summary>
        FF_STATUS = 0x17,
    }
}