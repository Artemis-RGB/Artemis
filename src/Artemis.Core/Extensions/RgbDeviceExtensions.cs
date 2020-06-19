using System.Text;
using RGB.NET.Core;

namespace Artemis.Core.Extensions
{
    public static class RgbDeviceExtensions
    {
        public static string GetDeviceIdentifier(this IRGBDevice rgbDevice)
        {
            var builder = new StringBuilder();
            builder.Append(rgbDevice.DeviceInfo.DeviceName);
            builder.Append('-');
            builder.Append(rgbDevice.DeviceInfo.Manufacturer);
            builder.Append('-');
            builder.Append(rgbDevice.DeviceInfo.Model);
            builder.Append('-');
            builder.Append(rgbDevice.DeviceInfo.DeviceType);
            builder.Append('-');
            builder.Append(rgbDevice.DeviceInfo.Lighting);
            return builder.ToString();
        }
    }
}