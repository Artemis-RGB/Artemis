using RGB.NET.Core;

namespace Artemis.Core.Extensions
{
    public static class RgbDeviceExtensions
    {
        public static int GetDeviceHashCode(this IRGBDevice rgbDevice)
        {
            unchecked
            {
                var hashCode = rgbDevice.DeviceInfo.DeviceName?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (rgbDevice.DeviceInfo.Manufacturer?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (rgbDevice.DeviceInfo.Model?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (int) rgbDevice.DeviceInfo.DeviceType;
                hashCode = (hashCode * 397) ^ (int) rgbDevice.DeviceInfo.Lighting;
                return hashCode;
            }
        }
    }
}