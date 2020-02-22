using RGB.NET.Core;

namespace Artemis.Core.Extensions
{
    public static class RgbDeviceExtensions
    {
        public static string GetDeviceIdentifier(this IRGBDevice rgbDevice)
        {
            return rgbDevice.DeviceInfo.DeviceName +
                   "-" + rgbDevice.DeviceInfo.Manufacturer +
                   "-" + rgbDevice.DeviceInfo.Model +
                   "-" + rgbDevice.DeviceInfo.DeviceType +
                   "-" + rgbDevice.DeviceInfo.Lighting;
        }
    }
}