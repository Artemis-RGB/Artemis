using System.Text;
using RGB.NET.Core;
using SkiaSharp;

namespace Artemis.Core
{
    internal static class RgbDeviceExtensions
    {
        public static string GetDeviceIdentifier(this IRGBDevice rgbDevice)
        {
            StringBuilder builder = new();
            builder.Append(rgbDevice.DeviceInfo.DeviceName);
            builder.Append('-');
            builder.Append(rgbDevice.DeviceInfo.Manufacturer);
            builder.Append('-');
            builder.Append(rgbDevice.DeviceInfo.Model);
            builder.Append('-');
            builder.Append(rgbDevice.DeviceInfo.DeviceType);
            return builder.ToString();
        }
    }

    internal static class RgbRectangleExtensions
    {
        public static SKRect ToSKRect(this Rectangle rectangle)
        {
            return SKRect.Create(
                rectangle.Location.X,
                rectangle.Location.Y,
                rectangle.Size.Width,
                rectangle.Size.Height
            );
        }
    }
}