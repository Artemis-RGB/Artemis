using System.Windows.Media;

namespace Artemis.DeviceProviders
{
    public abstract class DeviceProvider
    {
        /// <summary>
        ///     Indicates the device type
        /// </summary>
        public DeviceType Type { get; set; }

        /// <summary>
        ///     Indicates whether or not the device can be updated
        /// </summary>
        public bool CanUse { get; set; }

        /// <summary>
        ///     Updates a non-keyboard to take the colours of the provided brush
        /// </summary>
        /// <param name="brush"></param>
        public abstract void UpdateDevice(Brush brush);

        /// <summary>
        ///     Tries to enable the device and updates CanUse accordingly
        /// </summary>
        public abstract bool TryEnable();

        /// <summary>
        ///     Disables the device
        /// </summary>
        public abstract void Disable();
    }

    public enum DeviceType
    {
        Keyboard,
        Mouse,
        Headset
    }
}