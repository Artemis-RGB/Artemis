using System.Drawing;
using System.Threading.Tasks;

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
        ///     Updates a non-keyboard to take the colours found in the provided bitmap
        /// </summary>
        /// <param name="bitmap"></param>
        public abstract void UpdateDevice(Bitmap bitmap);

        /// <summary>
        ///     Tries to enable the device and updates CanUse accordingly
        /// </summary>
        public abstract bool TryEnable();

        /// <summary>
        ///     Disables the device
        /// </summary>
        public abstract void Disable();

        /// <summary>
        ///     Tries to enable the device and updates CanUse accordingly asynchronously
        /// </summary>
        /// <returns></returns>
        public Task<bool> TryEnableAsync()
        {
            return Task.Run(() => TryEnable());
        }
    }

    public enum DeviceType
    {
        Keyboard,
        Mouse,
        Headset,
        Generic,
        Mousemat
    }
}